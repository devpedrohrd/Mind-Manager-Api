
using Microsoft.EntityFrameworkCore;
using Mind_Manager.Infrastructure.Persistence;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager;

public class PsychologistRepository : IPsychologist
{
    public readonly ApplicationDbContext _context;

    public PsychologistRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<PsychologistProfile?> CreateAsync(PsychologistProfile createDto)
    {
        _context.PsychologistProfiles.Add(createDto);
        return await Task.FromResult(createDto);
    }

    public Task<bool> UpdateAsync(Guid userId, UpdatePsychologistCommand updateDto)
    {
        var profileExists =  this.GetByIdAsync(userId).Result;
        if (profileExists is null)
            return Task.FromResult(false);
        
        if (updateDto.Specialty != null)
            profileExists.Specialty = updateDto.Specialty;

        if(updateDto.Crp != null)
        profileExists.Crp = updateDto.Crp;

        return Task.FromResult(true);
    }

    public async Task<PsychologistProfile?> GetByUserIdAsync(Guid userId)
    {
        return await _context.PsychologistProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public Task<PsychologistProfile?> GetByIdAsync(Guid id)
    {
        return _context.PsychologistProfiles
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
