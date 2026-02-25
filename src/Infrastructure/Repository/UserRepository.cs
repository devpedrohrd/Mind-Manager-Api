using Microsoft.EntityFrameworkCore;
using Mind_Manager.Application.Mappers;
using Mind_Manager.Domain.Entities;
using Mind_Manager.Infrastructure.Persistence;
using Mind_Manager.Infrastructure.Specifications;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        return await Task.FromResult(user);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user is null) return false;
        _context.Users.Remove(user);

        return true;
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        return _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        return _context.Users
            .Include(u => u.PsychologistProfile)
            .Include(u => u.PatientProfile)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.PsychologistProfile)
            .Include(u => u.PatientProfile)
            .FirstOrDefaultAsync(u => u.Id == id);

        return user;
    }

    public async Task<SearchUsersResponse> SearchUsersAsync(SearchUsersQuery searchDto)
    {
        var query = _context.Users.AsQueryable();
        var filters = searchDto.Filters ?? new UserFilters();

        query = UserSpecification.ApplyFilters(query, filters);

        if (filters.IncludeProfile)
        {
            if (filters.Role == UserRole.Psychologist)
            {
                query = query.Include(u => u.PsychologistProfile);
            }
            else if (filters.Role == UserRole.Client)
            {
                query = query.Include(u => u.PatientProfile);
            }
            else
            {
                query = query
                    .Include(u => u.PsychologistProfile)
                    .Include(u => u.PatientProfile);
            }
        }

        int total = await query.CountAsync();

        query = UserSpecification.ApplyPagination(query, searchDto.Page, searchDto.Limit);

        var data = await query.ToArrayAsync();
        var totalPages = (int)Math.Ceiling((double)total / searchDto.Limit);
        var userResponses = data.Select(u => UserMapper.ToResponseDto(u)).ToList().AsReadOnly();

        return new SearchUsersResponse(
            Data: userResponses,
            Total: total,
            Page: searchDto.Page,
            Limit: searchDto.Limit,
            TotalPages: totalPages
        );
    }


    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return await Task.FromResult(user);
    }
}
