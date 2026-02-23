namespace Mind_Manager.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.src.Domain.DTO;

public class PatientProfile
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string? Registration { get; private set; }
    public string? Series { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public Gender Gender { get; private set; }
    public PatientType PatientType { get; private set; }
    public Education? Education { get; private set; }
    public Courses? Course { get; private set; }
    public CreatedBy CreatedBy { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    [NotMapped]
    public int? Age => BirthDate.HasValue
        ? DateTime.Today.Year - BirthDate.Value.Year -
          (DateTime.Today.DayOfYear < BirthDate.Value.DayOfYear ? 1 : 0)
        : null;

    public virtual User User { get; private set; } = null!;
    public virtual User? CreatedByUser { get; private set; }

    public List<PsychologicalDisorder> Disorders { get; private set; } = new();
    public List<Difficulty> Difficulties { get; private set; } = new();

    public virtual ICollection<Anamnesis> Anamneses { get; private set; } = new List<Anamnesis>();
    public virtual ICollection<Appointment> Appointments { get; private set; } = new List<Appointment>();
    public virtual ICollection<Session> Sessions { get; private set; } = new List<Session>();

    // EF Core
    // private PatientProfile() { }

    public PatientProfile(
        Guid userId,
        Gender gender,
        PatientType patientType,
        CreatedBy createdBy,
        Guid? createdByUserId = null,
        string? registration = null,
        string? series = null,
        DateTime? birthDate = null,
        Education? education = null,
        Courses? course = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("O ID do usuário não pode ser vazio.", nameof(userId));

        ValidateBirthDate(birthDate);

        Id = Guid.NewGuid();
        UserId = userId;
        Gender = gender;
        PatientType = patientType;
        CreatedBy = createdBy;
        CreatedByUserId = createdByUserId;
        Registration = registration;
        Series = series;
        BirthDate = birthDate;
        Education = education;
        Course = course;
    }

    public void UpdatePersonalInfo(
        string? registration,
        string? series,
        DateTime? birthDate,
        Gender gender,
        Education? education,
        Courses? course)
    {
        ValidateBirthDate(birthDate);

        Registration = registration;
        Series = series;
        BirthDate = birthDate;
        Gender = gender;
        Education = education;
        Course = course;
    }

    public void ChangePatientType(PatientType patientType)
    {
        PatientType = patientType;
    }

    public void AddDisorder(PsychologicalDisorder disorder)
    {
        if (Disorders.Contains(disorder))
            return;

        Disorders.Add(disorder);
    }

    public void RemoveDisorder(PsychologicalDisorder disorder)
    {
        if (!Disorders.Contains(disorder))
            throw new BusinessException($"DISORDER_NOT_FOUND: {disorder}");

        Disorders.Remove(disorder);
    }

    public void SetDisorders(IEnumerable<PsychologicalDisorder> disorders)
    {
        ArgumentNullException.ThrowIfNull(disorders);
        Disorders.Clear();
        Disorders.AddRange(disorders.Distinct());
    }

    public void AddDifficulty(Difficulty difficulty)
    {
        if (Difficulties.Contains(difficulty))
            return;

        Difficulties.Add(difficulty);
    }

    public void RemoveDifficulty(Difficulty difficulty)
    {
        if (!Difficulties.Contains(difficulty))
            throw new BusinessException($"DIFFICULTY_NOT_FOUND: {difficulty}");

        Difficulties.Remove(difficulty);
    }

    public void SetDifficulties(IEnumerable<Difficulty> difficulties)
    {
        ArgumentNullException.ThrowIfNull(difficulties);
        Difficulties.Clear();
        Difficulties.AddRange(difficulties.Distinct());
    }

    public bool HasDisorder(PsychologicalDisorder disorder) => Disorders.Contains(disorder);

    public bool HasDifficulty(Difficulty difficulty) => Difficulties.Contains(difficulty);

    public bool IsMinor() => Age.HasValue && Age.Value < 18;

    private static void ValidateBirthDate(DateTime? birthDate)
    {
        if (birthDate.HasValue && birthDate.Value.Date > DateTime.Today)
            throw new ArgumentException("A data de nascimento não pode ser no futuro.", nameof(birthDate));
    }

    public PatientProfileResponse ToDto() => new(
        Id: this.Id,
        UserId: this.UserId,
        Gender: this.Gender.ToString(),
        PatientType: this.PatientType.ToString(),
        CreatedBy: this.CreatedBy.ToString(),
        CreatedByUserId: this.CreatedByUserId,
        Registration: this.Registration,
        Series: this.Series,
        BirthDate: this.BirthDate,
        Age: this.Age,
        Education: this.Education?.ToString(),
        Course: this.Course?.ToString(),
        Disorders: this.Disorders.Select(d => d.ToString()).ToList(),
        Difficulties: this.Difficulties.Select(d => d.ToString()).ToList()
    );
}