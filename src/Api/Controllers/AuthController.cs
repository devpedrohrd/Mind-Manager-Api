using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IUserService _userService;


    public AuthController(IAuthService authService, ILogger<AuthController> logger, IUserService userService)
    {
        _logger = logger;
        _authService = authService;
        _userService = userService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserCommand createUserDto)
    {
        try
        {
            _logger.LogInformation("Criando novo usuário");

            var user = await _userService.CreateAsync(createUserDto);

            // 201 Created com location header apontando para o novo recurso
            return CreatedAtAction(nameof(Create), new { id = user.Id }, user);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning("Erro de negócio ao criar usuário: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] LoginCommand authDto)
    {
        try
        {
            _logger.LogInformation("Autenticando usuário.");
            var tokens = await _authService.AuthenticateAsync(authDto);
            return Ok(tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao autenticar usuário com email: {Email}", authDto.Email);
            return BadRequest(new { message = "INVALID_CREDENTIALS" });
        }
    }

    [Authorize]
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthenticationResponse>> RefreshToken([FromBody] RefreshTokenCommand refreshToken)
    {
        try
        {
            _logger.LogInformation("Atualizando token de acesso.");
            var tokens = await _authService.RefreshTokenAsync(refreshToken.RefreshToken);
            return Ok(tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar token de acesso.");
            return BadRequest(new { message = "INVALID_REFRESH_TOKEN" });
        }
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordCommand forgotPasswordDto)
    {
        try
        {
            _logger.LogInformation("Iniciando processo de recuperação de senha para email: {Email}", forgotPasswordDto.Email);
            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
            if (result)
            {
                return Ok(new { message = "PASSWORD_RESET_EMAIL_SENT" });
            }
            else
            {
                return BadRequest(new { message = "FAILED_TO_SEND_PASSWORD_RESET_EMAIL" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar processo de recuperação de senha para email: {Email}", forgotPasswordDto.Email);
            return BadRequest(new { message = "FAILED_TO_SEND_PASSWORD_RESET_EMAIL" });
        }
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordCommand resetPasswordDto)
    {
        try
        {
            _logger.LogInformation("Redefinindo senha para token: {Token}", resetPasswordDto.Token);
            var result = await _authService.ResetPasswordAsync(resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (result)
            {
                return Ok(new { message = "PASSWORD_RESET_SUCCESS" });
            }
            else
            {
                return BadRequest(new { message = "INVALID_OR_EXPIRED_TOKEN" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao redefinir senha para token: {Token}", resetPasswordDto.Token);
            return BadRequest(new { message = "FAILED_TO_RESET_PASSWORD" });
        }

    }
}