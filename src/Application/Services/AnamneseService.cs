using System;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.Domain.Interfaces;
using Mind_Manager.src.Domain.DTO;
using Mind_Manager.src.Domain.Interfaces;

namespace Mind_Manager.src.Application.Services;

public interface IAnamneseService
{
    Task<AnamnesisResponse> CreateAnamnesisAsync(CreateAnamnesisCommand command, Guid userIdRequesting);
    Task<AnamnesisResponse?> GetAnamnesisByIdAsync(Guid anamnesisId, Guid userIdRequesting);
    Task<bool> UpdateAnamnesisAsync(Guid anamnesisId, UpdateAnamnesisCommand command, Guid userIdRequesting);
    Task<bool> DeleteAnamnesisAsync(Guid anamnesisId, Guid userIdRequesting);
}
public class AnamneseService(IAnamnesis anamnesisRepository, IUnitOfWork unitOfWork, IPatientService patientService, IPsychologistService psychologistService) : IAnamneseService
{
    private readonly IAnamnesis _anamnesisRepository = anamnesisRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPatientService _patientService = patientService;
    private readonly IPsychologistService _psychologistService = psychologistService;

    public async Task<AnamnesisResponse> CreateAnamnesisAsync(CreateAnamnesisCommand command, Guid userIdRequesting)
    {
        var psychologist = await _psychologistService.GetByUserIdAsync(userIdRequesting)
            ?? throw new UnauthorizedAccessException("Perfil de psicólogo não encontrado.");

        _ = await _patientService.GetByIdAsync(command.PatientId) ?? throw new NotFoundException("Patient not found.");

        var anamnesis = new Anamnesis(
            command.PatientId,
            psychologist.Id,
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

    public async Task<bool> DeleteAnamnesisAsync(Guid anamnesisId, Guid userIdRequesting)
    {
        var anamnesis = await _anamnesisRepository.GetAnamnesisByIdAsync(anamnesisId)
            ?? throw new NotFoundException("Anamnesis not found.");

        await ValidateOwnershipAsync(anamnesis, userIdRequesting);

        var result = await _anamnesisRepository.DeleteAnamnesisAsync(anamnesisId);
        if (result)
            await _unitOfWork.SaveChangesAsync();
        return result;
    }

    public async Task<AnamnesisResponse?> GetAnamnesisByIdAsync(Guid anamnesisId, Guid userIdRequesting)
    {
        var anamnesis = await _anamnesisRepository.GetAnamnesisByIdAsync(anamnesisId);
        if (anamnesis is null) return null;

        await ValidateOwnershipAsync(anamnesis, userIdRequesting);

        return anamnesis.ToResponseDto();
    }

    public async Task<bool> UpdateAnamnesisAsync(Guid anamnesisId, UpdateAnamnesisCommand command, Guid userIdRequesting)
    {
        var existingAnamnesis = await _anamnesisRepository.GetAnamnesisByIdAsync(anamnesisId)
            ?? throw new NotFoundException("Anamnesis not found.");

        await ValidateOwnershipAsync(existingAnamnesis, userIdRequesting);

        existingAnamnesis.UpdateAnamnesis(
            command.FamilyHistory ?? existingAnamnesis.FamilyHistory,
            command.Infancy ?? existingAnamnesis.Infancy,
            command.Adolescence ?? existingAnamnesis.Adolescence,
            command.Illnesses ?? existingAnamnesis.Illnesses,
            command.Accompaniment ?? existingAnamnesis.Accompaniment);

        await _anamnesisRepository.UpdateAnamnesisAsync(existingAnamnesis.Id, existingAnamnesis);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Valida se o psicólogo autenticado é o criador da anamnese.
    /// </summary>
    private async Task ValidateOwnershipAsync(Anamnesis anamnesis, Guid userIdRequesting)
    {
        var psychologist = await _psychologistService.GetByUserIdAsync(userIdRequesting)
            ?? throw new UnauthorizedAccessException("Perfil de psicólogo não encontrado.");

        if (anamnesis.CreatedByPsychologistId != psychologist.Id)
            throw new UnauthorizedException("Você não tem permissão para acessar esta anamnese.");
    }
}

