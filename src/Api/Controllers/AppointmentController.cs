using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Mind_Manager.src.Application.Services;
using Mind_Manager.src.Domain.DTO;
using Mind_Manager.Domain.Exceptions;

namespace Mind_Manager.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentController : ControllerBase
{
    private readonly ILogger<AppointmentController> _logger;
    private readonly IAppointmentService _appointmentService;
    private readonly IUserLoggedHandller _userLoggedHandller;

    public AppointmentController(ILogger<AppointmentController> logger, IAppointmentService appointmentService, IUserLoggedHandller userLoggedHandller)
    {
        _logger = logger;
        _appointmentService = appointmentService;
        _userLoggedHandller = userLoggedHandller;

    }

    [Authorize(Roles = "Admin,Psychologist")]
    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchAppointmentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchAppointments([FromQuery] SearchAppointmentsQuery query)
    {
        var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);

        if (userId is null || userRole is null)
        {
            throw new UnauthorizedAccessException("User ID or role not found.");
        }

        var result = await _appointmentService.GetAppointmentsByFilterAsync(
            query, 
            userId.Value, 
            userRole == UserRole.Psychologist.ToString()
        );
        _logger.LogInformation("Usuário {UserId} com role {Role} buscando appointments com filtros", userId, userRole);
        
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Psychologist")]
    [HttpGet("period/{period}")]
    [ProducesResponseType(typeof(SearchAppointmentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAppointmentsByPeriod(
        string period,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] Status? status = null)
    {
        var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);

        if (userId is null || userRole is null)
        {
            throw new UnauthorizedAccessException("User ID or role not found.");
        }

        var (startDate, endDate) = period.ToLower() switch
        {
            "today" => (DateTime.Today, DateTime.Today.AddDays(1).AddTicks(-1)),
            "week" => GetCurrentWeekRange(),
            "month" => GetCurrentMonthRange(),
            _ => throw new ArgumentException("Período inválido. Use: today, week, month")
        };

        var filters = new AppointmentFilters(
            StartDate: startDate,
            EndDate: endDate,
            Status: status
        );

        var query = new SearchAppointmentsQuery(
            Filters: filters,
            Page: page,
            Limit: limit,
            SortBy: "AppointmentDate",
            SortDescending: false
        );

        var result = await _appointmentService.GetAppointmentsByFilterAsync(
            query,
            userId.Value,
            userRole == UserRole.Psychologist.ToString()
        );

        _logger.LogInformation("Usuário {UserId} buscando appointments do período {Period}", userId, period);
        return Ok(result);
    }

    private static (DateTime start, DateTime end) GetCurrentWeekRange()
    {
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7).AddTicks(-1);
        return (startOfWeek, endOfWeek);
    }

    private static (DateTime start, DateTime end) GetCurrentMonthRange()
    {
        var today = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);
        return (startOfMonth, endOfMonth);
    }

    [Authorize(Roles = "Psychologist, Client")]
    [HttpGet("my-appointments/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<AppointmentResponse[]> GetMyPsychologistAppointments(Guid id)
    {
        var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);

        if (userId is null || userRole is null)
        {
            throw new UnauthorizedAccessException("User ID or role not found.");
        }

        _logger.LogInformation("Psychologist {UserId} buscando seus agendamentos", userId);

        if (userRole == UserRole.Psychologist.ToString())
        {
            return await _appointmentService.GetAppointmentsByPsychologistIdAsync(id);
        }
        if (userRole == UserRole.Client.ToString())
        {
            return await _appointmentService.GetAppointmentsByPatientIdAsync(id);
        }
        else
        {
            throw new UnauthorizedAccessException("User role not authorized to access appointments.");
        }
    }

    [Authorize(Roles = "Admin,Psychologist")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<AppointmentResponse> CreateAppointment([FromBody] CreateAppointmentCommand appointmentDto)
    {

        var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);

        if (userRole == UserRole.Client.ToString())
        {
            throw new UnauthorizedAccessException("User role not authorized to create appointments.");
        }

        if (userId is null)
        {
            throw new UnauthorizedAccessException("User ID not found.");
        }
        appointmentDto = appointmentDto with { PsychologistId = userId.Value };
        var result = await _appointmentService.CreateAppointmentAsync(appointmentDto);
        _logger.LogInformation("Usuário {UserId} com role {Role} criando agendamento", userId, userRole);
        return result;
    }


    [Authorize(Roles = "Admin,Psychologist")]
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAppointment(Guid id, [FromBody] UpdateAppointmentCommand appointmentDto)
    {
        var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);

        if (userRole == UserRole.Client.ToString())
        {
            throw new UnauthorizedAccessException("User role not authorized to update appointments.");
        }

        var appointmentUpdated = await _appointmentService.UpdateAppointmentAsync(id, appointmentDto, userId ?? Guid.Empty, userRole == UserRole.Psychologist.ToString());

        if (appointmentUpdated)
        {
            _logger.LogInformation("Usuário {UserId} com role {Role} atualizando agendamento {AppointmentId}", userId, userRole, id);
            return Ok(new { message = $"Agendamento {id} atualizado com sucesso." });
        }
        else
        {
            return NotFound(new { message = $"Agendamento {id} não encontrado." });
        }
    }

    [Authorize(Roles = "Admin,Psychologist")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAppointment(Guid id)
    {
        var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);
        _logger.LogInformation("Admin deletando agendamento {AppointmentId}", id);
        var result = await _appointmentService.DeleteAppointmentAsync(id, userId ?? Guid.Empty, userRole == UserRole.Psychologist.ToString());
        return result ? Ok(new { message = $"Agendamento {id} deletado com sucesso." }) : NotFound(new { message = $"Agendamento {id} não encontrado." });
    }

    [Authorize(Roles = "Admin,Psychologist,Client")]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointmentById(Guid id)
    {
        var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);

        if (userId is null || userRole is null)
        {
            throw new UnauthorizedAccessException("User ID or role not found.");
        }

        try
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id, userId.Value, userRole);
            _logger.LogInformation("Usuário {UserId} buscando agendamento {AppointmentId}", userId, id);
            return Ok(appointment);
        }
        catch (NotFoundException)
        {
            return NotFound(new { message = $"Agendamento {id} não encontrado." });
        }
        catch (UnauthorizedException)
        {
            return Forbid("Você não tem permissão para acessar este agendamento.");
        }
    }
}
