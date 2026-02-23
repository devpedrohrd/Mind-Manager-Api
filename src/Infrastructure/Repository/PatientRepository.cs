using Mind_Manager.Domain.Entities;
using Mind_Manager.Infrastructure.Persistence;
using Mind_Manager.src.Domain.DTO;
using Mind_Manager.src.Domain.Interfaces;
using Mind_Manager.src.Infrastructure.Specifications;

namespace Mind_Manager.src.Infrastructure.Repository;

public class PatientRepository : IPatient
{
    private readonly ApplicationDbContext _context;

    public PatientRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<PatientProfile> CreatePatientProfileAsync(PatientProfile createPatientProfileDto)
    {
        _context.PatientProfiles.Add(createPatientProfileDto);
        return await Task.FromResult(createPatientProfileDto);
    }

    public async Task<PatientProfile?> GetPatientProfileByIdAsync(Guid patientId)
    {
        var patientProfile = _context.PatientProfiles.FirstOrDefault(p => p.Id == patientId);
        return await Task.FromResult(patientProfile);
    }

    public async Task<PatientProfile?> GetPatientProfileByUserIdAsync(Guid userId)
    {
        var patientProfile = _context.PatientProfiles.FirstOrDefault(p => p.UserId == userId);
        return await Task.FromResult(patientProfile);
    }

    public async Task<SearchPatientsResponse?> GetPatientsByFilterAsync(SearchPatientsQuery filters)
    {
        var query = _context.PatientProfiles.AsQueryable();
        query = PatientSpecification.ApplyFilters(query, filters);

        // Calcular total antes da paginação
        var total = query.Count();
        var totalPages = (int)Math.Ceiling(total / (double)filters.Limit);

        // Aplicar paginação
        query = PatientSpecification.ApplyPagination(query, filters.Page, filters.Limit);

        var patients = query.Select(p => p.ToDto()).ToList();

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
        var profiles = _context.PatientProfiles.ToList();
        return await Task.FromResult(profiles);
    }

    public async Task<PatientProfile> UpdatePatientProfileAsync(PatientProfile updatePatientProfileDto)
    {
        _context.PatientProfiles.Update(updatePatientProfileDto);
        await _context.SaveChangesAsync();

        return updatePatientProfileDto;
    }
}