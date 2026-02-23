using System.ComponentModel.DataAnnotations;

namespace Mind_Manager.src.Domain.DTO;

// === Authentication Command DTOs (Input) ===
public record LoginCommand
(
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password
);

public record RefreshTokenCommand
(
    [Required] string RefreshToken
);

public record ForgotPasswordCommand
(
    [Required, EmailAddress, MaxLength(255)] string Email
);

public record ResetPasswordCommand
(
    [Required] string Token,
    [Required, MinLength(6), MaxLength(100)] string NewPassword
);

// === Authentication Response DTOs (Output) ===
public record AuthenticationResponse
(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);

public record PasswordResetResponse
(
    bool Success,
    string Message
);