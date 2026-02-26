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

    /// <summary>
    /// Buscar usuários com filtros e paginação
    /// </summary>
    /// <param name="searchDto">Filtros para busca de usuários</param>
    /// <returns>Retorna lista de usuários encontrados</returns>
    /// <remarks>
    /// Administradores podem buscar todos os usuários. Psicólogos podem buscar pacientes relacionados a eles e administradores. Pacientes só podem buscar a si mesmos.
    /// </remarks>
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

    /// <summary>
    /// Buscar usuário por ID
    /// </summary>
    /// <param name="id">ID do usuário a ser buscado</param>
    /// <returns>Retorna o usuário encontrado ou erro</returns>
    /// <remarks>
    /// Administradores podem buscar qualquer usuário. Psicólogos podem buscar pacientes relacionados a eles e administradores. Pacientes só podem buscar a si mesmos.
    /// </remarks>
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

    /// <summary>
    /// Atualizar dados de um usuário
    /// </summary>
    /// <param name="id">ID do usuário a ser atualizado</param>
    /// <param name="updateUserDto">Dados a serem atualizados</param>
    /// <returns>Retorna o usuário atualizado ou erro</returns>
    /// <remarks>
    /// Administradores podem atualizar qualquer usuário. Psicólogos podem atualizar pacientes relacionados a eles e administradores. Pacientes só podem atualizar a si mesmos.
    /// </remarks>
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

    /// <summary>
    /// Excluir um usuário por ID
    /// </summary>
    /// <param name="id">ID do usuário a ser excluído</param>
    /// <returns>Retorna status de sucesso ou erro</returns>
    /// <remarks>
    /// Administradores podem excluir qualquer usuário. Psicólogos podem excluir pacientes relacionados a eles e administradores. Pacientes só podem excluir a si mesmos.
    /// </remarks>
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