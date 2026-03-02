using Microsoft.EntityFrameworkCore;
using Mind_Manager.Domain.Entities;
using Mind_Manager.Infrastructure.Persistence;
using Mind_Manager.src.Domain.DTO;
using Mind_Manager.src.Domain.Interfaces;
using Mind_Manager.src.Infrastructure.Specifications;

namespace Mind_Manager.src.Infrastructure.Repository;

public class PatientRepository(ApplicationDbContext context) : IPatient
{
    private readonly ApplicationDbContext _context = context;

    public async Task<PatientProfile> CreatePatientProfileAsync(PatientProfile createPatientProfileDto)
    {
        _context.PatientProfiles.Add(createPatientProfileDto);
        return await Task.FromResult(createPatientProfileDto);
    }

    public async Task<PatientProfile?> GetPatientProfileByIdAsync(Guid patientId)
    {
        return await _context.PatientProfiles.FirstOrDefaultAsync(p => p.Id == patientId);
    }

    public async Task<PatientProfile?> GetPatientProfileByUserIdAsync(Guid userId)
    {
        return await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<SearchPatientsResponse?> GetPatientsByFilterAsync(SearchPatientsQuery filters)
    {
        var query = _context.PatientProfiles.AsQueryable();
        query = PatientSpecification.ApplyFilters(query, filters);

        // Calcular total antes da paginação
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)filters.Limit);

        // Aplicar paginação
        query = PatientSpecification.ApplyPagination(query, filters.Page, filters.Limit);

        var patients = await query.Select(p => p.ToDto()).ToListAsync();

        return await Task.FromResult(new SearchPatientsResponse(
            Data: patients.AsReadOnly(),
            Total: total,
            Page: filters.Page,
            Limit: filters.Limit,
            TotalPages: totalPages
        ));
    }

    public async Task<IEnumerable<PatientProfile>> GetAllPatientProfilesAsync()
    {
        return await _context.PatientProfiles.ToListAsync();
    }

    public async Task<PatientProfile> UpdatePatientProfileAsync(PatientProfile updatePatientProfileDto)
    {
        _context.PatientProfiles.Update(updatePatientProfileDto);
        return await Task.FromResult(updatePatientProfileDto);
    }
}