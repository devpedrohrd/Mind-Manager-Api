using Mind_Manager;
using Mind_Manager.Domain.Entities;

public class PsychologistProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Crp { get; set; } = null!;
    public string Specialty { get; set; } = null!;

    public virtual User User { get; set; } = null!;
    public virtual ICollection<Appointment> Appointments { get; set; } = [];
    public virtual ICollection<Session> Sessions { get; set; } = [];

    public void UpdateProfile(string? crp, string? specialty)
    {
        if (!string.IsNullOrWhiteSpace(crp)) Crp = crp;
        if (!string.IsNullOrWhiteSpace(specialty)) Specialty = specialty;
    }

    public bool CanBeModifiedBy(Guid requestingUserId, UserRole requestingUserRole)
    {
        return requestingUserRole == UserRole.Admin || UserId == requestingUserId;
    }
}