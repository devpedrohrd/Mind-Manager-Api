using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager;

public interface IPsychologist
{
    Task<PsychologistProfile?> GetByUserIdAsync(Guid userId);

    Task<PsychologistProfile?> GetByIdAsync(Guid id);
    Task<PsychologistProfile?> CreateAsync(PsychologistProfile createDto);
    Task<bool> UpdateAsync(Guid userId, UpdatePsychologistCommand updateDto);
}