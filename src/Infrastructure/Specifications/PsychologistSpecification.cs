using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager;

public static class PsychologistSpecification
{
    public static IQueryable<PsychologistProfile> ApplyFilters(IQueryable<PsychologistProfile> query, PsychologistFilters filters)
    {
        if (filters == null)
        {
            return query;
        }
        if (!string.IsNullOrEmpty(filters.Specialty))
        {
            query = query.Where(p => p.Specialty.ToLower().Contains(filters.Specialty.ToLower()));
        }

        if(!string.IsNullOrEmpty(filters.Crp))
        {
            query = query.Where(p => p.Crp.ToLower().Contains(filters.Crp.ToLower()));
        }

        if (filters.Id.HasValue)
            query = query.Where(u => u.Id.Equals(filters.Id.Value));

        if(filters.UserId.HasValue)
            query = query.Where(u => u.UserId.Equals(filters.UserId.Value));

        return query;
    }

    public static IQueryable<PsychologistProfile> ApplyPagination(IQueryable<PsychologistProfile> query, int page, int limit)
    {
        return query
            .Skip((page - 1) * limit)
            .Take(limit);
    }


}