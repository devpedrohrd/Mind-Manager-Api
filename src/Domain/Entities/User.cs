namespace Mind_Manager.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public UserRole Role { get; set; } = UserRole.Client;

    public virtual PsychologistProfile? PsychologistProfile { get; set; }
    public virtual PatientProfile? PatientProfile { get; set; }

    public void UpdateProfile(string? name, string? email, string? phone)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;
        if (!string.IsNullOrWhiteSpace(email)) Email = email;
        if (!string.IsNullOrWhiteSpace(phone)) Phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanBeModifiedBy(Guid requestingUserId, UserRole requestingUserRole)
    {
        return requestingUserRole == UserRole.Admin || Id == requestingUserId;
    }

    public bool CanChangeRoleOrStatus(UserRole requestingUserRole)
    {
        return requestingUserRole == UserRole.Admin;
    }
}
