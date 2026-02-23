using System;
using Mind_Manager.Domain.Entities;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.Domain.Interfaces;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager.src.Application.Services;

public interface IPatientService
{
    Task<PatientProfileResponse> CreateAsync(CreatePatientProfileCommand createDto);
    Task<PatientProfileResponse> UpdateAsync(Guid id, UpdatePatientProfileCommand updateDto, Guid? userId = null, string? userRole = null);
    Task<PatientProfileResponse?> GetByIdAsync(Guid id);
    Task<PatientProfileResponse?> GetByUserIdAsync(Guid userId);
    Task<SearchPatientsResponse?> GetByFilterAsync(SearchPatientsQuery filters, Guid? userId = null, string? userRole = null);
}

public class PatientService : IPatientService
{
    private readonly IUnitOfWork _unitOfWork;

    public PatientService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<PatientProfileResponse> CreateAsync(CreatePatientProfileCommand createDto)
    {
        _ = await _unitOfWork.Users.GetByIdAsync(createDto.UserId) ?? throw new BusinessException("USER_NOT_FOUND");

        var existingProfile = await _unitOfWork.Patients.GetPatientProfileByIdAsync(createDto.UserId);
        if (existingProfile != null) throw new BusinessException("USER_ALREADY_HAS_PROFILE");

        var birthDate = createDto.BirthDate.HasValue
            ? DateTime.SpecifyKind(createDto.BirthDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        var patientProfile = new PatientProfile(
            userId: createDto.UserId,
            gender: createDto.Gender,
            patientType: createDto.PatientType,
            createdBy: createDto.CreatedBy,
            createdByUserId: createDto.CreatedByUserId,
            birthDate: birthDate,
            education: createDto.Education,
            registration: createDto.Registration,
            series: createDto.Series,
            course: createDto.Course
        );

        if (createDto.Disorders is not null)
            patientProfile.SetDisorders(createDto.Disorders);

        if (createDto.Difficulties is not null)
            patientProfile.SetDifficulties(createDto.Difficulties);

        await _unitOfWork.Patients.CreatePatientProfileAsync(patientProfile);
        await _unitOfWork.CommitTransactionAsync();

        return patientProfile.ToDto();
    }

    public async Task<SearchPatientsResponse?> GetByFilterAsync(SearchPatientsQuery filters, Guid? userId = null, string? userRole = null)
    {
        // Se não for Admin ou Psychologist, restringir busca aos próprios pacientes
        if (userId.HasValue && userRole != null)
        {
            var currentFilters = filters.Filters ?? new PatientFilters();
            
            if (userRole == UserRole.Client.ToString())
            {
                // Cliente só pode ver seu próprio perfil
                filters = filters with 
                { 
                    Filters = currentFilters with { UserId = userId.Value } 
                };
            }
            else if (userRole == UserRole.Psychologist.ToString())
            {
                // Psicólogo só pode ver pacientes que ele criou
                filters = filters with 
                { 
                    Filters = currentFilters with { CreatedByUserId = userId.Value } 
                };
            }   
        }

        return await _unitOfWork.Patients.GetPatientsByFilterAsync(filters);
    }

    public async Task<PatientProfileResponse?> GetByIdAsync(Guid id)
    {
        var patientProfile = await _unitOfWork.Patients.GetPatientProfileByIdAsync(id);
        return patientProfile?.ToDto();
    }

    public async Task<PatientProfileResponse?> GetByUserIdAsync(Guid userId)
    {
        var patientProfile = await _unitOfWork.Patients.GetPatientProfileByIdAsync(userId);
        return patientProfile?.ToDto();
    }

    public async Task<PatientProfileResponse> UpdateAsync(Guid id, UpdatePatientProfileCommand updateDto, Guid? userId = null, string? userRole = null)
    {
        var patientProfile = await _unitOfWork.Patients.GetPatientProfileByIdAsync(id) ?? throw new BusinessException("PATIENT_PROFILE_NOT_FOUND");
        if (userRole == UserRole.Client.ToString() && patientProfile.CreatedByUserId != userId)
            throw new BusinessException("UNAUTHORIZED");

        var birthDate = updateDto.BirthDate.HasValue
            ? DateTime.SpecifyKind(updateDto.BirthDate.Value, DateTimeKind.Utc)
            : patientProfile.BirthDate;

        patientProfile.UpdatePersonalInfo(
            registration: updateDto.Registration ?? patientProfile.Registration,
            series: updateDto.Series ?? patientProfile.Series,
            birthDate: birthDate,
            gender: updateDto.Gender ?? patientProfile.Gender,
            education: updateDto.Education ?? patientProfile.Education,
            course: updateDto.Course ?? patientProfile.Course
        );

        if (updateDto.PatientType is not null)
            patientProfile.ChangePatientType(updateDto.PatientType.Value);

        if (updateDto.DisordersToAdd is not null)
            foreach (var disorder in updateDto.DisordersToAdd)
                patientProfile.AddDisorder(disorder);

        if (updateDto.DisordersToRemove is not null)
            foreach (var disorder in updateDto.DisordersToRemove)
                patientProfile.RemoveDisorder(disorder);

        if (updateDto.DifficultiestoAdd is not null)
            foreach (var difficulty in updateDto.DifficultiestoAdd)
                patientProfile.AddDifficulty(difficulty);

        if (updateDto.DifficultiestoRemove is not null)
            foreach (var difficulty in updateDto.DifficultiestoRemove)
                patientProfile.RemoveDifficulty(difficulty);

        await _unitOfWork.Patients.UpdatePatientProfileAsync(patientProfile);
        await _unitOfWork.CommitTransactionAsync();

        return patientProfile.ToDto();
    }
}