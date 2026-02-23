using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager;

[ApiController]
[Route("api/users")]
public class UserControllers : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserControllers> _logger;

    private readonly IUserLoggedHandller _userLoggedHandller;
    public UserControllers(IUserService userService, ILogger<UserControllers> logger, IUserLoggedHandller userLoggedHandller)
    {
        _logger = logger;
        _userService = userService;
        _userLoggedHandller = userLoggedHandller;
    }

    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = "AllUsers")]
    public async Task<ActionResult<SearchUsersResponse>> Search([FromQuery] SearchUsersQuery searchDto)
    {
        try
        {
            var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);

            _logger.LogInformation("Usuário com role {UserRole} buscando usuários", userRole);

            var users = await _userService.SearchAsync(searchDto, userId, userRole);
            return Ok(users);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning("Erro de negócio ao buscar usuários: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = "AllUsers")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id)
    {
        try
        {
            var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);
            _logger.LogInformation("Buscando usuário com ID");

            var user = await _userService.GetByIdAsync(id, userId, userRole);
            return Ok(user);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Usuário não encontrado: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning("Acesso negado para usuário: {Id}", id);
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Policy = "AllUsers")]
    public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserCommand updateUserDto)
    {
        try
        {
            var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);
            _logger.LogInformation("Atualizando usuário com ID: {Id}", id);

            var user = await _userService.UpdateAsync(id, updateUserDto, userId, userRole);
            return Ok(user);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Usuário não encontrado para atualização: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning("Acesso negado para atualização do usuário: {Id}", id);
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Policy = "AllUsers")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);

            _logger.LogInformation("Excluindo usuário pelo ID");
            var deleted = await _userService.DeleteAsync(id, userId, userRole);
            if (!deleted)
            {
                _logger.LogWarning("Usuário não encontrado para exclusão");
                return NotFound(new { message = "USER_NOT_FOUND" });
            }
            return NoContent();
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning("Erro de negócio ao excluir usuário: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning("Acesso negado para exclusão do usuário: {Id}", id);
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }
}