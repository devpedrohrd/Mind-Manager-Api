using System;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.Domain.Interfaces;
using Mind_Manager.src.Domain.DTO;
using Mind_Manager.src.Domain.Interfaces;

namespace Mind_Manager.src.Application.Services;

public interface IAnamneseService
{
    Task<AnamnesisResponse> CreateAnamnesisAsync(CreateAnamnesisCommand command, bool isPsychologist);
    Task<AnamnesisResponse?> GetAnamnesisByIdAsync(Guid anamnesisId, bool isPsychologist);
    Task<bool> UpdateAnamnesisAsync(Guid anamnesisId, UpdateAnamnesisCommand command, bool isPsychologist);
    Task<bool> DeleteAnamnesisAsync(Guid anamnesisId, bool isPsychologist);
}
public class AnamneseService(IAnamnesis anamnesisRepository, IUnitOfWork unitOfWork, IPatientService patientService) : IAnamneseService
{
    private readonly IAnamnesis _anamnesisRepository = anamnesisRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPatientService _patientService = patientService;
    public async Task<AnamnesisResponse> CreateAnamnesisAsync(CreateAnamnesisCommand command, bool isPsychologist)
    {
        if (!isPsychologist)
        {
            throw new UnauthorizedAccessException("Psychologist profile not found.");
        }

        _ = await _patientService.GetByIdAsync(command.PatientId) ?? throw new NotFoundException("Patient not found.");

        var anamnesis = new Anamnesis(
            command.PatientId,
            command.FamilyHistory,
            command.Infancy,
            command.Adolescence,
            command.Illnesses,
            command.Accompaniment
        )
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var createdAnamnesis = await _anamnesisRepository.CreateAnamnesisAsync(anamnesis);
        await _unitOfWork.SaveChangesAsync();
        return createdAnamnesis.ToResponseDto();
    }

    public async Task<bool> DeleteAnamnesisAsync(Guid anamnesisId, bool isPsychologist)
    {
        if (!isPsychologist)
        {
            throw new UnauthorizedAccessException("Psychologist profile not found.");
        }

        var result = await _anamnesisRepository.DeleteAnamnesisAsync(anamnesisId);
        if (result)
            await _unitOfWork.SaveChangesAsync();
        return result;
    }

    public async Task<AnamnesisResponse?> GetAnamnesisByIdAsync(Guid anamnesisId, bool isPsychologist)
    {
        if (!isPsychologist)
        {
            throw new UnauthorizedAccessException("Psychologist profile not found.");
        }

        var anamnesis = await _anamnesisRepository.GetAnamnesisByIdAsync(anamnesisId);
        return anamnesis?.ToResponseDto();
    }

    public async Task<bool> UpdateAnamnesisAsync(Guid anamnesisId, UpdateAnamnesisCommand command, bool isPsychologist)
    {
        if (!isPsychologist)
        {
            throw new UnauthorizedAccessException("Psychologist profile not found.");
        }

        var existingAnamnesis = await _anamnesisRepository.GetAnamnesisByIdAsync(anamnesisId) ?? throw new NotFoundException("Anamnesis not found.");

        existingAnamnesis.UpdateAnamnesis(
            command.FamilyHistory ?? existingAnamnesis.FamilyHistory,
            command.Infancy ?? existingAnamnesis.Infancy,
            command.Adolescence ?? existingAnamnesis.Adolescence,
            command.Illnesses ?? existingAnamnesis.Illnesses,
            command.Accompaniment ?? existingAnamnesis.Accompaniment);

        await _anamnesisRepository.UpdateAnamnesisAsync(existingAnamnesis.Id,existingAnamnesis);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

