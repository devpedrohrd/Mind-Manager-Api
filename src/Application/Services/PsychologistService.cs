using Mind_Manager.Application.Authorization;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.Domain.Interfaces;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager;

public interface IPsychologistService
{
    Task<PsychologistResponse> CreateAsync(CreatePsychologistCommand createDto);
    Task<PsychologistResponse> UpdateAsync(Guid id, UpdatePsychologistCommand updateDto, Guid? userId = null, string? userRole = null);
    Task<PsychologistResponse?> GetByUserIdAsync(Guid userId);

    Task<PsychologistResponse?> GetByIdAsync(Guid id);
}

public class PsychologistService : IPsychologistService
{
    private readonly IUnitOfWork _unitOfWork;

    public PsychologistService(IUnitOfWork unitOfWork, IAuthorizationService authorizationService)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PsychologistResponse> CreateAsync(CreatePsychologistCommand createDto)
    {
        if (createDto.UserId == null)
            throw new ArgumentNullException(nameof(createDto), "UserId cannot be null.");

        if (string.IsNullOrWhiteSpace(createDto.Crp))
            throw new BusinessException("CRP_REQUIRED");
            
        if (string.IsNullOrWhiteSpace(createDto.Specialty))
            throw new BusinessException("SPECIALTY_REQUIRED");

        var user = await _unitOfWork.Psychologists.GetByUserIdAsync(createDto.UserId.Value);

        if (user != null)
            throw new BusinessException("USER_ALREADY_HAS_PROFILE");

        var psychologist = new PsychologistProfile
        {
            Id = Guid.NewGuid(),
            UserId = createDto.UserId.Value,
            Crp = createDto.Crp,
            Specialty = createDto.Specialty,
        };

        await _unitOfWork.Psychologists.CreateAsync(psychologist);
        await _unitOfWork.SaveChangesAsync();

        return new PsychologistResponse(
            Id: psychologist.Id,
            UserId: psychologist.UserId,
            Specialty: psychologist.Specialty,
            Crp: psychologist.Crp
        );
    }

    public async Task<PsychologistResponse> UpdateAsync(Guid id, UpdatePsychologistCommand updateDto, Guid? userId = null, string? userRole = null)
    {
        var psychologist = await _unitOfWork.Psychologists.GetByIdAsync(id);
        if (psychologist == null)
            throw new BusinessException("PSYCHOLOGIST_NOT_FOUND");

        if (updateDto.Crp != null)
            psychologist.Crp = updateDto.Crp;

        if (updateDto.Specialty != null)
            psychologist.Specialty = updateDto.Specialty;

        await _unitOfWork.Psychologists.UpdateAsync(id, updateDto);
        await _unitOfWork.SaveChangesAsync();

        return new PsychologistResponse(
            Id: psychologist.Id,
            UserId: psychologist.UserId,
            Specialty: psychologist.Specialty,
            Crp: psychologist.Crp
        );
    }

    public async Task<PsychologistResponse?> GetByUserIdAsync(Guid userId)
    {
        var psychologist = await _unitOfWork.Psychologists.GetByUserIdAsync(userId);
        if (psychologist == null)
            return null;

        return new PsychologistResponse(
            Id: psychologist.Id,
            UserId: psychologist.UserId,
            Specialty: psychologist.Specialty,
            Crp: psychologist.Crp
        );
    }

    public async Task<PsychologistResponse?> GetByIdAsync(Guid id)
    {
        var psychologist = await _unitOfWork.Psychologists.GetByIdAsync(id);
        if (psychologist == null)
            return null;

        return new PsychologistResponse(
            Id: psychologist.Id,
            UserId: psychologist.UserId,
            Specialty: psychologist.Specialty,
            Crp: psychologist.Crp
        );
    }
}