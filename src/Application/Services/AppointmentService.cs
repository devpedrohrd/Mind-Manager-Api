using Microsoft.AspNetCore.Mvc;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.Domain.Interfaces;
using Mind_Manager.src.Application.Mappers;
using Mind_Manager.src.Domain.DTO;
using Mind_Manager.src.Domain.Interfaces;

namespace Mind_Manager.src.Application.Services;

public interface IAppointmentService
{
    Task<AppointmentResponse> CreateAppointmentAsync(CreateAppointmentCommand createAppointmentDto);
    Task<bool> DeleteAppointmentAsync(Guid appointmentId, Guid userIdRequesting, bool isPsychologist);
    Task<AppointmentResponse> GetAppointmentByIdAsync(Guid appointmentId, Guid userIdRequesting, string userRole);
    Task<SearchAppointmentsResponse> GetAppointmentsByFilterAsync(SearchAppointmentsQuery filters, Guid userIdRequesting, bool isPsychologist);
    Task<AppointmentResponse[]> GetAppointmentsByPatientIdAsync(Guid patientId);
    Task<AppointmentResponse[]> GetAppointmentsByPsychologistIdAsync(Guid psychologistId);
    Task<bool> UpdateAppointmentAsync(Guid appointmentId, UpdateAppointmentCommand updateAppointmentDto, Guid userIdRequesting, bool isPsychologist);
}

public class AppointmentService(IAppointment appointmentRepository, IUnitOfWork unitOfWork, IPatientService patientService, IPsychologistService psychologistService) : IAppointmentService
{
    private readonly IAppointment _appointmentRepository = appointmentRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private readonly IPatientService _patientService = patientService;
    private readonly IPsychologistService _psychologistService = psychologistService;

    public async Task<AppointmentResponse> CreateAppointmentAsync(CreateAppointmentCommand createAppointmentDto)
    {
        var psychologist = await _psychologistService.GetByUserIdAsync(createAppointmentDto.PsychologistId ?? Guid.Empty)
            ?? throw new NotFoundException("Psychologist not found.");
        var patient = await _patientService.GetByUserIdAsync(createAppointmentDto.PatientId)
            ?? throw new NotFoundException("Patient not found.");

        var appointment = new Appointment(
            psychologistId: psychologist.Id, // Usar o PsychologistProfile.Id, não o UserId
            patientId: patient.Id, // Usar o PatientProfile.Id, não o UserId
            appointmentDate: createAppointmentDto.AppointmentDate,
            status: createAppointmentDto.Status,
            type: createAppointmentDto.Type,
            activityType: createAppointmentDto.ActivityType,
            reason: createAppointmentDto.Reason,
            observation: createAppointmentDto.Observation,
            objective: createAppointmentDto.Objective
        );

        var result = await _appointmentRepository.CreateAppointmentAsync(appointment);
        await _unitOfWork.SaveChangesAsync();
        return AppointmentMapper.ToResponseDto(result.Value ?? throw new Exception("Error creating appointment."));
    }

    public async Task<bool> DeleteAppointmentAsync(Guid appointmentId, Guid userIdRequesting, bool isPsychologist)
    {
        // Verificar se o appointment existe e se o usuário tem permissão
        AppointmentResponse appointment;
        try
        {
            appointment = await GetAppointmentByIdAsync(appointmentId, userIdRequesting, isPsychologist ? UserRole.Psychologist.ToString() : UserRole.Admin.ToString());
        }
        catch (NotFoundException)
        {
            return false;
        }
        catch (UnauthorizedException)
        {
            throw; // Re-throw para que o controller possa tratar
        }

        var result = await _appointmentRepository.DeleteAppointmentAsync(appointmentId);
        if (result)
            await _unitOfWork.SaveChangesAsync();
        return result;
    }

    public async Task<AppointmentResponse> GetAppointmentByIdAsync(Guid appointmentId, Guid userIdRequesting, string userRole)
    {
        var result = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId) ?? throw new NotFoundException("Appointment not found.");

        // Validação de permissão
        if (userRole != UserRole.Admin.ToString())
        {
            if (userRole == UserRole.Psychologist.ToString())
            {
                var psychologistProfile = await _psychologistService.GetByUserIdAsync(userIdRequesting);
                if (psychologistProfile == null || result.PsychologistId != psychologistProfile.Id)
                    throw new UnauthorizedException("You don't have permission to access this appointment.");
            }
            else if (userRole == UserRole.Client.ToString())
            {
                var patientProfile = await _patientService.GetByUserIdAsync(userIdRequesting);
                if (patientProfile == null || result.PatientId != patientProfile.Id)
                    throw new UnauthorizedException("You don't have permission to access this appointment.");
            }
        }

        return AppointmentMapper.ToResponseDto(result);
    }

    public async Task<AppointmentResponse[]> GetAppointmentsByPatientIdAsync(Guid patientId)
    {
        var result = await _appointmentRepository.GetAppointmentsByPatientIdAsync(patientId);

        if (result.Value == null)
            throw new NotFoundException("No appointments found for this patient.");

        return [.. AppointmentMapper.ToResponseDtoList(result.Value)];
    }

    public async Task<AppointmentResponse[]> GetAppointmentsByPsychologistIdAsync(Guid psychologistId)
    {
        var result = await _appointmentRepository.GetAppointmentsByPsychologistIdAsync(psychologistId);
        if (result.Value == null)
            throw new NotFoundException("No appointments found for this psychologist.");

        return [.. AppointmentMapper.ToResponseDtoList(result.Value)];
    }

    public async Task<bool> UpdateAppointmentAsync(Guid appointmentId, UpdateAppointmentCommand updateAppointmentDto, Guid userIdRequesting, bool isPsychologist)
    {
        var appointmentResult = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
        if (appointmentResult == null)
            throw new NotFoundException("Appointment not found.");

        var existingAppointment = appointmentResult;

        var psychologistProfile = await _psychologistService.GetByUserIdAsync(userIdRequesting);
        
        if (psychologistProfile == null || existingAppointment.PsychologistId != psychologistProfile.Id || !isPsychologist)
            throw new UnauthorizedException("You are not authorized to update this appointment.");

        appointmentResult.ToUpdateCommand(
            updateAppointmentDto.Status ?? existingAppointment.Status,
            updateAppointmentDto.Type ?? existingAppointment.Type,
            updateAppointmentDto.AppointmentDate ?? existingAppointment.AppointmentDate,
            updateAppointmentDto.ActivityType ?? existingAppointment.ActivityType,
            updateAppointmentDto.Reason ?? existingAppointment.Reason,
            updateAppointmentDto.Observation ?? existingAppointment.Observation,
            updateAppointmentDto.Objective ?? existingAppointment.Objective
        );

        await _appointmentRepository.UpdateAppointmentAsync(appointmentResult);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<SearchAppointmentsResponse> GetAppointmentsByFilterAsync(SearchAppointmentsQuery filters, Guid userIdRequesting, bool isPsychologist)
    {
        if (!isPsychologist)
        {
            return await _appointmentRepository.GetAppointmentsByFilterAsync(filters);
        }

        var psychologistProfile = await _psychologistService.GetByUserIdAsync(userIdRequesting) ?? throw new UnauthorizedException("Psychologist profile not found.");
        var currentFilters = filters.Filters ?? new AppointmentFilters();
        currentFilters = currentFilters with { PsychologistId = psychologistProfile.Id };
        
        var searchQuery = filters with { Filters = currentFilters };
        return await _appointmentRepository.GetAppointmentsByFilterAsync(searchQuery);
    }
}
