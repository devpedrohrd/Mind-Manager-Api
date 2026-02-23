using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Mind_Manager.Domain.Entities;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.Domain.Interfaces;
using Mind_Manager.Domain.Validators;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager;

public interface IAuthService
{
    Task<AuthenticationResponse> AuthenticateAsync(LoginCommand authDto);
    Task<AuthenticationResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
}

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IUserValidator _validator;
    private readonly IEmailService _emailService;

    public AuthService(
        IUnitOfWork unitOfWork, 
        IConfiguration configuration, 
        IUserValidator validator,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _validator = validator;
        _emailService = emailService;
    }
    public async Task<AuthenticationResponse> AuthenticateAsync(LoginCommand authDto)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(authDto.Email) ?? throw new NotFoundException("EMAIL_NOT_FOUND");

        if (user.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(authDto.Password, user.PasswordHash))
            throw new BusinessException("PASSWORD_INCORRECT");

        var accessToken = GenerateToken(user, TimeSpan.FromMinutes(15));
        var newRefreshToken = GenerateToken(user, TimeSpan.FromDays(7), isRefreshToken: true);
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        return new AuthenticationResponse(accessToken, newRefreshToken, expiresAt);
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        try
        {
            _validator.ValidateEmail(email);
            
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user is null) 
                throw new NotFoundException("EMAIL_NOT_FOUND");

            var resetToken = GeneratePasswordResetToken(user);
            var baseUrl = _configuration["App:BaseUrl"] ?? "https://localhost:5001";
            var resetLink = $"{baseUrl}/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";

            await _emailService.SendPasswordResetEmailAsync(email, resetLink);
            return true;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BusinessException("ERROR_SENDING_EMAIL", ex);
        }
    }

    public async Task<AuthenticationResponse> RefreshTokenAsync(string refreshToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

        try
        {
            var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out SecurityToken validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken && jwtToken.Claims.Any(c => c.Type == "isRefreshToken" && c.Value == "true"))
            {
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim is null)
                    throw new BusinessException("INVALID_REFRESH_TOKEN");

                var userId = Guid.Parse(userIdClaim.Value);
                var user = await _unitOfWork.Users.GetByIdAsync(userId) ?? throw new NotFoundException("USER_NOT_FOUND");

                var accessToken = GenerateToken(user, TimeSpan.FromMinutes(15));
                var refreshTokenn = GenerateToken(user, TimeSpan.FromDays(7), isRefreshToken: true);
                var expiresAt = DateTime.UtcNow.AddMinutes(15);

                return new AuthenticationResponse(accessToken, refreshTokenn, expiresAt);
            }
            else
            {
                throw new BusinessException("INVALID_REFRESH_TOKEN");
            }
        }
        catch (Exception ex)
        {
            throw new BusinessException("INVALID_REFRESH_TOKEN", ex);
        }
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        _validator.ValidatePassword(newPassword);
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out SecurityToken validatedToken);

            var tokenTypeClaim = principal.Claims.FirstOrDefault(c => c.Type == "tokenType")?.Value;
            if (tokenTypeClaim != "passwordReset")
                throw new BusinessException("INVALID_RESET_TOKEN");

            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim is null)
                throw new BusinessException("INVALID_RESET_TOKEN");

            var userId = Guid.Parse(userIdClaim.Value);
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user is null)
                throw new NotFoundException("USER_NOT_FOUND");

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
                throw new BusinessException("PASSWORD_INVALID");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, 12);
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BusinessException("INVALID_RESET_TOKEN", ex);
        }
    }

    private string GeneratePasswordResetToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("tokenType", "passwordReset")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1), // Token válido por 1 hora
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateToken(User user, TimeSpan expiration, bool isRefreshToken = false)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

        var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email),
        new(ClaimTypes.Role, user.Role.ToString())
    };

        if (isRefreshToken)
        {
            claims.Add(new Claim("isRefreshToken", "true"));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(expiration),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

}
