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

    /// <summary>
    /// Criar um novo usuário
    /// </summary>
    /// <param name="createUserDto">Dados para criação do usuário</param>
    /// <returns>Retorna o usuário criado ou erro</returns>
    /// <remarks>
    /// Este endpoint é público e pode ser acessado por qualquer pessoa para criar uma conta.
    /// O campo "Role" no CreateUserCommand deve ser preenchido com "Patient" para criar uma conta de paciente. Contas de psicólogo e administrador devem ser criadas por um administrador usando o endpoint de criação de usuário com autenticação.
    /// </remarks>
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

    /// <summary>
    /// Autenticar um usuário e obter tokens de acesso e refresh
    /// </summary>
    /// <param name="authDto"></param>
    /// <returns></returns>
    /// <remarks>
    /// Este endpoint é público e pode ser acessado por qualquer pessoa para autenticar e obter tokens. O campo "Email" deve conter o email do usuário e o campo "Password" deve conter a senha do usuário.
    /// </remarks>
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

    /// <summary>
    /// Atualizar token de acesso usando um token de refresh válido
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    /// <remarks>
    /// Este endpoint é protegido e requer autenticação. O campo "RefreshToken" deve conter um token de refresh válido que foi emitido anteriormente para o usuário.
    /// </remarks>
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

    /// <summary>
    /// Solicitar recuperação de senha por email
    /// </summary>
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

    /// <summary>
    /// Redefinir senha de um usuário usando um token de recuperação válido e uma nova senha
    /// </summary>
    /// <param name="resetPasswordDto">Dados para redefinir senha</param>
    /// <returns>Retorna status de sucesso ou erro</returns>
    /// <remarks>
    /// Este endpoint é público e pode ser acessado por qualquer pessoa para redefinir a senha de um usuário. O campo "Token" deve conter um token de recuperação válido que foi emitido anteriormente para o usuário, e o campo "NewPassword" deve conter a nova senha que o usuário deseja definir.
    /// </remarks>
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