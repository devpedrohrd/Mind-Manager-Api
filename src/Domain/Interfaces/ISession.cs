namespace Mind_Manager.src.Domain.Interfaces;
using Mind_Manager.Domain.Entities;
using Mind_Manager.src.Domain.DTO;
public interface ISession
{
    Task<Session> CreateSessionAsync(Session createSessionDto);
    Task<Session?> GetSessionByIdAsync(Guid sessionId);
    Task<SearchSessionsResponse> GetSessionsByFilterAsync(SearchSessionsQuery filters);
    Task<bool> UpdateSessionAsync(Session updateSessionDto);
    Task<bool> DeleteSessionAsync(Guid sessionId);
}