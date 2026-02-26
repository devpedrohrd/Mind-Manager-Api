using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Mind_Manager.src.Application.Services;
using Mind_Manager.src.Domain.DTO;
using Mind_Manager.Domain.Exceptions;
using System.ComponentModel;

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

    /// <summary>
    /// Buscar agendamentos com filtros e paginação
    /// </summary>
    /// <param name="query">Filtros para busca de agendamentos</param>
    /// <returns>Retorna lista de agendamentos encontrados</returns>
    /// <remarks>
    /// Administradores podem buscar todos os agendamentos. Psicólogos podem buscar agendamentos relacionados a eles e administradores. Pacientes só podem buscar agendamentos relacionados a eles.
    /// </remarks>
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

    /// <summary>
    /// Buscar agendamentos por período (hoje, esta semana, este mês)
    /// </summary>
    /// <param name="period">Período para busca (today, week, month)</param>
    /// <param name="status">Status do agendamento para filtrar (opcional)</param>
    /// <param name="page">Número da página para paginação (opcional, padrão: 1)</param>
    /// <param name="limit">Número de itens por página para paginação (opcional, padrão: 10)</param>
    /// <returns>Retorna lista de agendamentos encontrados</returns>
    /// <remarks>
    /// Administradores podem buscar todos os agendamentos. Psicólogos podem buscar agendamentos relacionados a eles e administradores. Pacientes só podem buscar agendamentos relacionados a eles.
    /// </remarks>
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

        var todayUtc = DateTime.UtcNow.Date;
        var (startDate, endDate) = period.ToLower() switch
        {
            "today" => (todayUtc, todayUtc.AddDays(1).AddTicks(-1)),
            "week" => GetCurrentWeekRange(todayUtc),
            "month" => GetCurrentMonthRange(todayUtc),
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

    private static (DateTime start, DateTime end) GetCurrentWeekRange(DateTime todayUtc)
    {
        var startOfWeek = todayUtc.AddDays(-(int)todayUtc.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7).AddTicks(-1);
        return (startOfWeek, endOfWeek);
    }

    private static (DateTime start, DateTime end) GetCurrentMonthRange(DateTime todayUtc)
    {
        var startOfMonth = new DateTime(todayUtc.Year, todayUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);
        return (startOfMonth, endOfMonth);
    }

    /// <summary>
    /// Buscar agendamentos relacionados ao psicólogo ou paciente logado
    /// </summary>
    /// <param name="id">ID do psicólogo ou paciente para buscar agendamentos relacionados</param>
    /// <returns>Retorna lista de agendamentos encontrados</returns>
    /// <remarks>
    /// Psicólogos podem buscar agendamentos relacionados a eles e administradores. Pacientes só podem buscar agendamentos relacionados a eles.
    /// </remarks>
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

    /// <summary>
    /// Criar um novo agendamento
    /// </summary>
    /// <param name="appointmentDto">Dados para criação do agendamento</param>
    /// <returns>Retorna o agendamento criado ou erro</returns>
    /// <remarks>
    /// Somente psicólogos e administradores podem criar agendamentos. Pacientes não têm permissão para criar agendamentos.
    /// </remarks>
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

    /// <summary>
    /// Atualizar um agendamento existente
    /// </summary>
    /// <param name="id">ID do agendamento a ser atualizado</param>
    /// <param name="appointmentDto">Dados para atualização do agendamento</param>
    /// <returns>Retorna o agendamento atualizado ou erro</returns>
    /// <remarks>
    /// Somente psicólogos e administradores podem atualizar agendamentos. Pacientes não têm permissão para atualizar agendamentos.
    /// </remarks>
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

    /// <summary>
    /// Excluir um agendamento por ID
    /// </summary>
    /// <param name="id">ID do agendamento a ser excluído</param>
    /// <returns>Retorna status de sucesso ou erro</returns>
    /// <remarks>
    /// Somente psicólogos e administradores podem excluir agendamentos. Pacientes não têm permissão para excluir agendamentos.
    /// </remarks>
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

    /// <summary>
    /// Buscar um agendamento por ID
    /// </summary>
    /// <param name="id">ID do agendamento a ser buscado</param>
    /// <returns>Retorna o agendamento encontrado ou erro</returns>
    /// <remarks>
    /// Administradores podem buscar qualquer agendamento. Psicólogos podem buscar agendamentos relacionados a eles e administradores. Pacientes só podem buscar agendamentos relacionados a eles.
    /// </remarks>
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
