using System.ComponentModel.DataAnnotations;

namespace Mind_Manager.src.Domain.DTO;

public record CreateUserCommand(
    [Required, MaxLength(255)] string Name,
    [EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(6)] string Password,
    [Phone, MaxLength(20)] string Phone
);

public record UpdateUserCommand(
    [MaxLength(255)] string? Name = null,
    [EmailAddress, MaxLength(255)] string? Email = null,
    [Phone, MaxLength(20)] string? Phone = null,
    bool? IsActive = null,
    UserRole? Role = null
);

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    bool IsActive,
    string Role,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    UserProfileResponse? Profile = null
);

public record UserProfileResponse(
    PsychologistResponse? PsychologistProfile = null,
    PatientProfileResponse? PatientProfile = null
);

public record UserFilters(
    Guid? Id = null,
    string? Name = null,
    [EmailAddress] string? Email = null,
    string? Phone = null,
    bool? IsActive = null,
    UserRole? Role = null,
    bool IncludeProfile = false
);

public record SearchUsersQuery(
    UserFilters? Filters = null,
    int Page = 1,
    int Limit = 10
)
{
    public SearchUsersQuery() : this(new UserFilters()) { }
};

public record SearchUsersResponse(
    IReadOnlyCollection<UserResponse> Data,
    int Total,
    int Page,
    int Limit,
    int TotalPages
);
