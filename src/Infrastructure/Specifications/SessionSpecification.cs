namespace Mind_Manager.src.Infrastructure.Specifications;
using Mind_Manager.Domain.Entities;
using Mind_Manager.src.Domain.DTO;

public class SessionSpecification
{
    public static IQueryable<Session> ApplyFilters(IQueryable<Session> query, SearchSessionsQuery filters)
    {
        if(filters.Id.HasValue)
            query = query.Where(p => p.Id == filters.Id.Value);
        if(filters.PatientId.HasValue)
            query = query.Where(p => p.PatientId == filters.PatientId.Value);
        if(filters.PsychologistId.HasValue)
            query = query.Where(p => p.PsychologistId == filters.PsychologistId.Value);
        if(filters.AppointmentId.HasValue)
            query = query.Where(p => p.AppointmentId == filters.AppointmentId.Value);
        if(filters.StartDate.HasValue)
        {
            var startDate = filters.StartDate.Value.Kind == DateTimeKind.Utc
                ? filters.StartDate.Value
                : DateTime.SpecifyKind(filters.StartDate.Value, DateTimeKind.Utc);
            query = query.Where(p => p.SessionDate >= startDate);
        }
        if(filters.EndDate.HasValue)
        {
            var endDate = filters.EndDate.Value.Kind == DateTimeKind.Utc
                ? filters.EndDate.Value
                : DateTime.SpecifyKind(filters.EndDate.Value, DateTimeKind.Utc);
            query = query.Where(p => p.SessionDate <= endDate);
        }

        return query;
    }

    public static IQueryable<Session> ApplySorting(IQueryable<Session> query, SearchSessionsQuery filters)
    {
        var sortBy = filters.SortBy?.ToLower() switch
        {
            "sessiondate" or _ => filters.SortDescending ? query.OrderByDescending(a => a.SessionDate) : query.OrderBy(a => a.SessionDate)
        };
        
        return sortBy.ThenBy(a => a.Id);
    }

    public static IQueryable<Session> ApplyPagination(IQueryable<Session> query, int page, int limit)
    {
        return query
            .Skip((page - 1) * limit)
            .Take(limit)
            .AsQueryable();
    }
}
