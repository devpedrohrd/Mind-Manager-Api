
using Microsoft.EntityFrameworkCore;
using Mind_Manager.Infrastructure.Persistence;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager;

public class PsychologistRepository(ApplicationDbContext context) : IPsychologist
{
    public readonly ApplicationDbContext _context = context;

    public async Task<PsychologistProfile?> CreateAsync(PsychologistProfile createDto)
    {
        _context.PsychologistProfiles.Add(createDto);
        return await Task.FromResult(createDto);
    }

    public async Task<bool> UpdateAsync(Guid userId, UpdatePsychologistCommand updateDto)
    {
        var profileExists = await this.GetByIdAsync(userId);
        if (profileExists is null)
            return false;
        
        if (updateDto.Specialty != null)
            profileExists.Specialty = updateDto.Specialty;

        if(updateDto.Crp != null)
        profileExists.Crp = updateDto.Crp;

        return true;
    }

    public async Task<PsychologistProfile?> GetByUserIdAsync(Guid userId)
    {
        return await _context.PsychologistProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<PsychologistProfile?> GetByIdAsync(Guid id)
    {
        return await _context.PsychologistProfiles
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
