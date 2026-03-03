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
    Task<AppointmentsPendingsResponse> GetPendingAppointmentsForPsychologistAsync(DateTime? startDate, DateTime? endDate, Guid userIdRequesting);
}

public class AppointmentService(IAppointment appointmentRepository,IPatientService patientService, IPsychologistService psychologistService,IEmailSchedule emailSchedule, IUnitOfWork unitOfWork) : IAppointmentService
{
    private readonly IAppointment _appointmentRepository = appointmentRepository;
    private readonly IPatientService _patientService = patientService;
    private readonly IPsychologistService _psychologistService = psychologistService;
    private readonly IEmailSchedule _emailSchedule = emailSchedule;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AppointmentResponse> CreateAppointmentAsync(CreateAppointmentCommand createAppointmentDto)
    {
        var psychologist = await _psychologistService.GetByUserIdAsync(createAppointmentDto.PsychologistId ?? Guid.Empty)
            ?? throw new NotFoundException("Psychologist not found.");

        Guid? patientProfileId = null;

        if (createAppointmentDto.Type == TypeAppointment.Session)
        {
            if (createAppointmentDto.PatientId == null || createAppointmentDto.PatientId == Guid.Empty)
                throw new ValidationException("O paciente é obrigatório para agendamentos do tipo Sessão.");

            var patient = await _patientService.GetByIdAsync(createAppointmentDto.PatientId.Value)
                ?? throw new NotFoundException("Patient not found.");
            patientProfileId = patient.Id;
        }
        else if (createAppointmentDto.PatientId != null && createAppointmentDto.PatientId != Guid.Empty)
        {
            // Para outros tipos, paciente é opcional
            var patient = await _patientService.GetByUserIdAsync(createAppointmentDto.PatientId.Value);
            patientProfileId = patient?.Id;
        }

        var appointment = new Appointment(
            psychologistId: psychologist.Id,
            patientId: patientProfileId,
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
        return AppointmentMapper.ToResponseDto(result);
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
        return [.. AppointmentMapper.ToResponseDtoList(result)];
    }

    public async Task<AppointmentResponse[]> GetAppointmentsByPsychologistIdAsync(Guid psychologistId)
    {
        var result = await _appointmentRepository.GetAppointmentsByPsychologistIdAsync(psychologistId);
        return [.. AppointmentMapper.ToResponseDtoList(result)];
    }

    public async Task<bool> UpdateAppointmentAsync(Guid appointmentId, UpdateAppointmentCommand updateAppointmentDto, Guid userIdRequesting, bool isPsychologist)
    {
        var existingAppointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId) ?? throw new NotFoundException("Appointment not found.");

        var psychologistProfile = await _psychologistService.GetByUserIdAsync(userIdRequesting);
        
        if (psychologistProfile == null || existingAppointment.PsychologistId != psychologistProfile.Id || !isPsychologist)
            throw new UnauthorizedException("You are not authorized to update this appointment.");

        // Alteração de status via State Machine
        if (updateAppointmentDto.Status.HasValue)
        {
            existingAppointment.ChangeStatus(updateAppointmentDto.Status.Value);
        }

        // Atualizar os demais campos editáveis
        existingAppointment.UpdateDetails(
            updateAppointmentDto.Type,
            updateAppointmentDto.AppointmentDate,
            updateAppointmentDto.ActivityType,
            updateAppointmentDto.Reason,
            updateAppointmentDto.Observation,
            updateAppointmentDto.Objective
        );

        // Se a data foi alterada ou o status mudou para Cancelado, invalida emails agendados
        var dateChanged = updateAppointmentDto.AppointmentDate.HasValue 
            && updateAppointmentDto.AppointmentDate.Value != existingAppointment.AppointmentDate;
        var wasCanceled = updateAppointmentDto.Status == Status.Canceled;

        if (dateChanged || wasCanceled)
        {
            await _emailSchedule.DeleteByAppointmentIdAsync(appointmentId);
        }

        await _appointmentRepository.UpdateAppointmentAsync(existingAppointment);
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

    public async Task<AppointmentsPendingsResponse> GetPendingAppointmentsForPsychologistAsync(DateTime? startDate, DateTime? endDate, Guid userIdRequesting)
    {
        return await _appointmentRepository.GetPendingAppointmentsForPsychologistAsync(startDate, endDate, userIdRequesting);
    }
}
