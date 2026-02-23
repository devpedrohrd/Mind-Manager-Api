using System;
using Mind_Manager.Domain.Entities;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager.src.Infrastructure.Specifications;

public class PatientSpecification
{
    public static IQueryable<PatientProfile> ApplyFilters(IQueryable<PatientProfile> query, SearchPatientsQuery filters)
    {
        var patientFilters = filters.Filters ?? new PatientFilters();
        
        if(patientFilters.Id.HasValue)
            query = query.Where(p => p.Id == patientFilters.Id.Value);
        if(patientFilters.UserId.HasValue)
            query = query.Where(p => p.UserId == patientFilters.UserId.Value);
        if (patientFilters.Gender != null)
            query = query.Where(p => p.Gender == patientFilters.Gender);
        if (patientFilters.PatientType != null)
            query = query.Where(p => p.PatientType == patientFilters.PatientType);
        if (patientFilters.CreatedBy != null)
            query = query.Where(p => p.CreatedBy == patientFilters.CreatedBy);
        if (patientFilters.CreatedByUserId.HasValue)
            query = query.Where(p => p.CreatedByUserId == patientFilters.CreatedByUserId.Value);
        return query;
    }

    public static IQueryable<PatientProfile> ApplyPagination(IQueryable<PatientProfile> query, int page, int limit)
    {
        return query
            .Skip((page - 1) * limit)
            .Take(limit);
    }

}
