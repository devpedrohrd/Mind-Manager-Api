using Microsoft.EntityFrameworkCore;
using Mind_Manager.Domain.Entities;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager.Infrastructure.Specifications;

public static class UserSpecification
{
    public static IQueryable<User> ApplyFilters(IQueryable<User> query, UserFilters filters)
    {
        if (filters.Id.HasValue)
            query = query.Where(u => u.Id == filters.Id.Value);

        if (!string.IsNullOrWhiteSpace(filters.Name))
            query = query.Where(u => u.Name.ToLower().Contains(filters.Name.ToLower()));

        if (!string.IsNullOrWhiteSpace(filters.Email))
            query = query.Where(u => u.Email.ToLower().Contains(filters.Email.ToLower()));

        if (!string.IsNullOrWhiteSpace(filters.Phone))
            query = query.Where(u => u.Phone.Contains(filters.Phone));

        if (filters.IsActive.HasValue)
            query = query.Where(u => u.IsActive == filters.IsActive.Value);

        if (filters.Role.HasValue)
        {
            if (Enum.TryParse<UserRole>(filters.Role.ToString(), true, out var role))
                query = query.Where(u => u.Role == role);
        }

        if (filters.IncludeProfile)
        {
            query = query.Include(u => u.PsychologistProfile)
                         .Include(u => u.PatientProfile);
        }

        return query;
    }

    public static IQueryable<User> ApplyPagination(IQueryable<User> query, int page, int limit)
    {
        return query
            .Skip((page - 1) * limit)
            .Take(limit);
    }
}
