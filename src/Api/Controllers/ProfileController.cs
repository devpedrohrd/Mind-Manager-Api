using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Mind_Manager.src.Application.Services;
using Mind_Manager.src.Domain.DTO;
using Mind_Manager.Domain.Exceptions;

namespace Mind_Manager.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ILogger<ProfileController> _logger;
    private readonly IPatientService _patientService;

    private readonly IPsychologistService _psychologistService;
    private readonly IUserLoggedHandller _userLoggedHandller;

    public ProfileController(ILogger<ProfileController> logger, IPatientService patientService, IPsychologistService psychologistService, IUserLoggedHandller userLoggedHandller)
    {
        _logger = logger;
        _patientService = patientService;
        _psychologistService = psychologistService;
        _userLoggedHandller = userLoggedHandller;
    }

    [HttpPost("patient")]
    [Authorize(Roles = "Psychologist,Admin")]
    [ProducesResponseType(typeof(PatientProfileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePatientProfile([FromBody] CreatePatientProfileCommand createDto)
    {
        try
        {
            var (loggedUserId, _) = _userLoggedHandller.GetUserIdAndRole(User);

            // Injeta o ID do usuário logado como criador do perfil
            var dtoWithCreator = createDto with { CreatedByUserId = loggedUserId };

            _logger.LogInformation("Criando perfil de paciente para usuário {UserId} por {CreatorId}", createDto.UserId, loggedUserId);
            var result = await _patientService.CreateAsync(dtoWithCreator);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar perfil de paciente: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Psychologist ou Admin pode atualizar perfil de paciente
    /// </summary>
    [Authorize(Roles = "Psychologist,Admin")]
    [HttpPatch("patient/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePatientProfile([FromRoute] Guid id, [FromBody] UpdatePatientProfileCommand patientProfileDto)
    {
        try
        {
            var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);

            _logger.LogInformation("Usuário {UserId} atualizando perfil de paciente {PatientId}", userId, id);

            var userUpdated = await _patientService.UpdateAsync(id, patientProfileDto, userId, userRole);

            return Ok(userUpdated);
        }
        catch (BusinessException ex)
        {
            _logger.LogError(ex, "Erro ao atualizar perfil de paciente: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("patients/search")]
    [Authorize(Roles = "Psychologist,Admin,Client")]
    [ProducesResponseType(typeof(SearchPatientsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SearchPatientProfiles(
        [FromQuery] Guid? id = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] Gender? gender = null,
        [FromQuery] PatientType? patientType = null,
        [FromQuery] CreatedBy? createdBy = null,
        [FromQuery] Guid? createdByUserId = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        try
        {
            var (loggedUserId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);

            _logger.LogInformation("Usuário {UserId} buscando perfis de pacientes com filtros", loggedUserId);

            var filters = new PatientFilters(
                Id: id,
                UserId: userId,
                Gender: gender,
                PatientType: patientType,
                CreatedBy: createdBy,
                CreatedByUserId: createdByUserId
            );

            var query = new SearchPatientsQuery(filters, page, limit);
            
            var profiles = await _patientService.GetByFilterAsync(query, loggedUserId, userRole);

            return Ok(profiles);
        }
        catch (BusinessException ex)
        {
            _logger.LogError(ex, "Erro ao buscar perfis de pacientes: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("psychologist")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Psychologist,Admin")]
    public async Task<ActionResult<PsychologistResponse>> Create([FromBody] CreatePsychologistCommand createPsychologistDto)
    {
        try
        {
            _logger.LogInformation("Criando novo perfil de psicólogo");
            var result = await _psychologistService.CreateAsync(createPsychologistDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Erro ao criar perfil de psicólogo: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (BusinessException ex)
        {
            _logger.LogError(ex, "Erro ao criar perfil de psicólogo: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize(Roles = "Psychologist,Admin")]
    [HttpPatch("psychologist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PsychologistResponse>> Update([FromBody] UpdatePsychologistCommand updatePsychologistDto)
    {
        try
        {
            var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);
            var psychologist = await _psychologistService.GetByUserIdAsync(userId!.Value);
            if (psychologist == null)
                return NotFound(new { error = "PSYCHOLOGIST_NOT_FOUND" });

            var result = await _psychologistService.UpdateAsync(psychologist.Id, updatePsychologistDto, userId, userRole);
            return Ok(result);
        }
        catch (BusinessException ex)
        {
            _logger.LogError(ex, "Erro ao atualizar perfil de psicólogo: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet()]
    [Authorize(Roles = "Psychologist,Client")]
    [ProducesResponseType(typeof(SearchPatientsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SearchPatientsResponse>> GetMyProfile([FromQuery] SearchPatientsQuery filters) {
        var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);

        var result = await _patientService.GetByFilterAsync(filters, userId, userRole);
        return Ok(result);
    }
}
