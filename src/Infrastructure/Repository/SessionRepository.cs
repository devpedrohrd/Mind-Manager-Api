using System;
using Microsoft.EntityFrameworkCore;
using Mind_Manager.Domain.Entities;
using Mind_Manager.Infrastructure.Persistence;
using Mind_Manager.src.Domain.DTO;
using Mind_Manager.src.Infrastructure.Specifications;
namespace Mind_Manager.src.Infrastructure.Repository;


public class SessionRepository(ApplicationDbContext context) : src.Domain.Interfaces.ISession
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Session> CreateSessionAsync(Session createSessionDto)
    {
        await _context.Sessions.AddAsync(createSessionDto);
        await _context.SaveChangesAsync();
        return createSessionDto;
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        var session = await GetSessionByIdAsync(sessionId);
        if (session is null) return false;
        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Session?> GetSessionByIdAsync(Guid sessionId)
    {
        return await _context.Sessions.FindAsync(sessionId);
    }

    public async Task<SearchSessionsResponse> GetSessionsByFilterAsync(SearchSessionsQuery filters)
    {
        var query = _context.Sessions.AsQueryable();

        query = SessionSpecification.ApplyFilters(query, filters);
        query = SessionSpecification.ApplySorting(query, filters);
        var totalItems = await query.CountAsync();
        var sessions = await SessionSpecification.ApplyPagination(query, filters.Page, filters.Limit).ToListAsync();
        var sessionResponses = sessions.Select(s => new SessionResponse(
            s.Id,
            s.PsychologistId,
            s.PatientId,
            s.AppointmentId,
            s.Complaint,
            s.Intervention,
            s.Referrals,
            s.SessionDate
        )).ToList();

        return new SearchSessionsResponse(
            sessionResponses,
            totalItems,
            filters.Page,
            filters.Limit,
            (int)Math.Ceiling(totalItems / (double)filters.Limit)
        );
    }

    public async Task<bool> UpdateSessionAsync(Session updateSessionDto)
    {
        _context.Sessions.Update(updateSessionDto);
        await _context.SaveChangesAsync();
        return true;
    }
}
