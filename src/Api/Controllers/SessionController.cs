using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mind_Manager.src.Application.Services;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager.src.Api.Controllers
{
    [Route("api/sessions")]
    [ApiController]
    public class SessionController(ILogger<SessionController> logger, ISessionService sessionService, Mind_Manager.IUserLoggedHandller userLoggedHandler) : ControllerBase
    {
        private readonly ILogger<SessionController> _logger = logger;
        private readonly ISessionService _sessionService = sessionService;
        private readonly Mind_Manager.IUserLoggedHandller _userLoggedHandler = userLoggedHandler;

        [HttpPost]
        [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Psychologist")]
        public async Task<SessionResponse> CreateSession([FromBody] CreateSessionCommand createSessionDto)
        {
            var createdSession = await _sessionService.CreateSessionAsync(createSessionDto);
            _logger.LogInformation("Session created with ID {SessionId} for psychologist {PsychologistId} and patient {PatientId}", createdSession.Id, createdSession.PsychologistId, createdSession.PatientId);
            return createdSession;
        }

        [HttpDelete("{sessionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin,Psychologist")]
        public async Task<IActionResult> DeleteSession(Guid sessionId)
        {
            var (userId, userRole) = _userLoggedHandler.GetUserIdAndRole(User);
            var result = await _sessionService.DeleteSessionAsync(sessionId, userId ?? Guid.Empty, userRole == UserRole.Psychologist.ToString());
            if (result)
                return Ok("Session deleted successfully.");
            return NotFound("Session not found.");
        }

        [HttpGet("{sessionId}")]
        [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin,Psychologist,Client")]
        public async Task<IActionResult> GetSessionById(Guid sessionId)
        {
            var (userId, userRole) = _userLoggedHandler.GetUserIdAndRole(User);
            var sessionUpdated = await _sessionService.GetSessionByIdAsync(sessionId, userId ?? Guid.Empty, userRole == UserRole.Psychologist.ToString());

            return Ok(sessionUpdated);
        }
        [HttpPatch("{sessionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin,Psychologist")]
        public async Task<IActionResult> UpdateSession(Guid sessionId, [FromBody] UpdateSessionCommand updateSessionDto)
        {
            var (userId, userRole) = _userLoggedHandler.GetUserIdAndRole(User);
            var result = await _sessionService.UpdateSessionAsync(sessionId, updateSessionDto, userId ?? Guid.Empty, userRole == UserRole.Psychologist.ToString());
            if (result)
                return Ok("Session updated successfully.");
            return NotFound("Session not found.");
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(SearchSessionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin,Psychologist,Client")]
        public async Task<IActionResult> SearchSessions([FromQuery] SearchSessionsQuery filters)
        {
            var (userId, userRole) = _userLoggedHandler.GetUserIdAndRole(User);

            if (userId is null || userRole is null)
            {
                throw new UnauthorizedAccessException("User ID or role not found.");
            }

            var result = await _sessionService.GetSessionsByFilterAsync(
                filters,
                userId.Value,
                userRole == UserRole.Psychologist.ToString()
            );

            _logger.LogInformation("Usuário {UserId} com role {Role} buscando sessions com filtros", userId, userRole);

            return Ok(result);
        }
    }

}