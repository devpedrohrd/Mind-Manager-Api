using System;
using Microsoft.EntityFrameworkCore;
using Mind_Manager.Infrastructure.Persistence;
using Mind_Manager.src.Domain.Interfaces;

namespace Mind_Manager.src.Infrastructure.Repository;

public class AnamnesisRepository (ApplicationDbContext _context) : IAnamnesis
{
    private readonly ApplicationDbContext _context = _context;

    public async Task<Anamnesis> CreateAnamnesisAsync(Anamnesis anamnesis)
    {
        await _context.Anamneses.AddAsync(anamnesis);
        return anamnesis;
    }

    public async Task<bool> DeleteAnamnesisAsync(Guid anamnesisId)
    {
        var anamnesis = await GetAnamnesisByIdAsync(anamnesisId);
        if (anamnesis is null) return false;
        _context.Anamneses.Remove(anamnesis);
        return true;
    }

    public async Task<Anamnesis?> GetAnamnesisByIdAsync(Guid anamnesisId)
    {
        return await _context.Anamneses.FindAsync(anamnesisId);
    }

    public async Task<IEnumerable<Anamnesis>> GetAnamnesisByPatientIdAsync(Guid patientId)
    {
        return await _context.Anamneses
            .Where(a => a.PatientId == patientId)
            .ToListAsync();
    }

    public Task<bool> UpdateAnamnesisAsync(Guid anamnesisId, Anamnesis updatedAnamnesis)
    {
        _context.Anamneses.Update(updatedAnamnesis);
        return Task.FromResult(true);
    }
}
