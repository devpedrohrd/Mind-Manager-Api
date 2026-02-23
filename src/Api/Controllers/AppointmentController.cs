// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization;
// using System.Security.Claims;

// namespace Mind_Manager.Api.Controllers;

// [ApiController]
// [Route("api/appointments")]
// public class AppointmentController : ControllerBase
// {
//     private readonly ILogger<AppointmentController> _logger;

//     public AppointmentController(ILogger<AppointmentController> logger)
//     {
//         _logger = logger;
//     }

//     /// <summary>
//     /// Admin pode ver todos os agendamentos
//     /// </summary>
//     [Authorize(Roles = "Admin")]
//     [HttpGet("all")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//     public async Task<IActionResult> GetAllAppointments()
//     {
//         _logger.LogInformation("Admin buscando todos os agendamentos");
//         // TODO: implementar busca no repositório
//         return Ok(new { message = "Lista de todos os agendamentos (Admin)" });
//     }

//     /// <summary>
//     /// Psychologist pode ver seus próprios agendamentos
//     /// </summary>
//     [Authorize(Roles = "Psychologist")]
//     [HttpGet("my-appointments")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//     public async Task<IActionResult> GetMyPsychologistAppointments()
//     {
//         var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
//         if (string.IsNullOrEmpty(userId))
//         {
//             return Unauthorized(new { message = "USER_ID_NOT_FOUND_IN_TOKEN" });
//         }

//         _logger.LogInformation("Psychologist {UserId} buscando seus agendamentos", userId);
//         // TODO: implementar busca filtrando por PsychologistId = userId
//         return Ok(new { message = $"Agendamentos do psicólogo {userId}" });
//     }

//     /// <summary>
//     /// Client pode ver seus próprios agendamentos
//     /// </summary>
//     [Authorize(Roles = "Client")]
//     [HttpGet("my-schedule")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//     public async Task<IActionResult> GetMyClientAppointments()
//     {
//         var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
//         if (string.IsNullOrEmpty(userId))
//         {
//             return Unauthorized(new { message = "USER_ID_NOT_FOUND_IN_TOKEN" });
//         }

//         _logger.LogInformation("Client {UserId} buscando seus agendamentos", userId);
//         // TODO: implementar busca filtrando por PatientId = userId
//         return Ok(new { message = $"Agendamentos do cliente {userId}" });
//     }

//     /// <summary>
//     /// Admin ou Psychologist podem criar agendamentos
//     /// </summary>
//     [Authorize(Roles = "Admin,Psychologist")]
//     [HttpPost]
//     [ProducesResponseType(StatusCodes.Status201Created)]
//     [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//     [ProducesResponseType(StatusCodes.Status403Forbidden)]
//     public async Task<IActionResult> CreateAppointment([FromBody] object appointmentDto)
//     {
//         var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//         var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

//         _logger.LogInformation("Usuário {UserId} com role {Role} criando agendamento", userId, userRole);
        
//         // TODO: implementar criação de agendamento
//         // Se for Psychologist, validar que está criando para si mesmo
        
//         return Created("/api/appointments/1", new { message = "Agendamento criado", userId, userRole });
//     }

//     /// <summary>
//     /// Admin ou o próprio Psychologist pode atualizar o agendamento
//     /// </summary>
//     [Authorize(Roles = "Admin,Psychologist")]
//     [HttpPut("{id}")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//     [ProducesResponseType(StatusCodes.Status403Forbidden)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     public async Task<IActionResult> UpdateAppointment(int id, [FromBody] object appointmentDto)
//     {
//         var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//         var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

//         // TODO: buscar appointment por id
//         // Validação simples: se não for Admin, verificar se o appointment pertence ao psychologist
//         // if (userRole != "Admin" && appointment.PsychologistId != Guid.Parse(userId))
//         // {
//         //     return Forbid(); // ou return StatusCode(403, new { message = "FORBIDDEN" });
//         // }

//         _logger.LogInformation("Usuário {UserId} atualizando agendamento {AppointmentId}", userId, id);
//         return Ok(new { message = $"Agendamento {id} atualizado" });
//     }

//     /// <summary>
//     /// Apenas Admin pode deletar agendamentos
//     /// </summary>
//     [Authorize(Roles = "Admin")]
//     [HttpDelete("{id}")]
//     [ProducesResponseType(StatusCodes.Status204NoContent)]
//     [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     public async Task<IActionResult> DeleteAppointment(int id)
//     {
//         _logger.LogInformation("Admin deletando agendamento {AppointmentId}", id);
//         // TODO: implementar deleção
//         return NoContent();
//     }

//     /// <summary>
//     /// Qualquer usuário autenticado pode ver detalhes de um agendamento específico
//     /// (com validação de que tem permissão para ver esse agendamento)
//     /// </summary>
//     [Authorize(Roles = "Admin,Psychologist,Client")]
//     [HttpGet("{id}")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//     [ProducesResponseType(StatusCodes.Status403Forbidden)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     public async Task<IActionResult> GetAppointmentById(int id)
//     {
//         var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//         var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

//         // TODO: buscar appointment por id
//         // Validação simples:
//         // - Admin pode ver qualquer appointment
//         // - Psychologist só pode ver se for dele (appointment.PsychologistId == userId)
//         // - Client só pode ver se for dele (appointment.PatientId == userId)
        
//         // if (userRole == "Psychologist" && appointment.PsychologistId != Guid.Parse(userId))
//         //     return Forbid();
//         // if (userRole == "Client" && appointment.PatientId != Guid.Parse(userId))
//         //     return Forbid();

//         _logger.LogInformation("Usuário {UserId} buscando agendamento {AppointmentId}", userId, id);
//         return Ok(new { message = $"Detalhes do agendamento {id}" });
//     }
// }
