using System.ComponentModel.DataAnnotations;

namespace Mind_Manager.src.Domain.DTO;

public record CreatePatientProfileCommand(
    Guid UserId,
    Gender Gender,
    PatientType PatientType,
    CreatedBy CreatedBy,
    Guid? CreatedByUserId = null,
    [MaxLength(100)] string? Registration = null,
    [MaxLength(50)] string? Series = null,
    DateTime? BirthDate = null,
    Education? Education = null,
    Courses? Course = null,
    List<PsychologicalDisorder>? Disorders = null,
    List<Difficulty>? Difficulties = null
);

public record UpdatePatientProfileCommand(
    Gender? Gender = null,
    PatientType? PatientType = null,
    [MaxLength(100)] string? Registration = null,
    [MaxLength(50)] string? Series = null,
    DateTime? BirthDate = null,
    Education? Education = null,
    Courses? Course = null,
    List<PsychologicalDisorder>? DisordersToAdd = null,
    List<PsychologicalDisorder>? DisordersToRemove = null,
    List<Difficulty>? DifficultiestoAdd = null,
    List<Difficulty>? DifficultiestoRemove = null
);

public record PatientProfileResponse(
    Guid Id,
    Guid UserId,
    string Gender,
    string PatientType,
    string CreatedBy,
    Guid? CreatedByUserId,
    string? Registration,
    string? Series,
    DateTime? BirthDate,
    int? Age,
    string? Education,
    string? Course,
    List<string> Disorders,
    List<string> Difficulties
);

public record PatientFilters(
    Guid? Id = null,
    Guid? UserId = null,
    Gender? Gender = null,
    PatientType? PatientType = null,
    CreatedBy? CreatedBy = null,
    Guid? CreatedByUserId = null
);

public record SearchPatientsQuery(
    PatientFilters? Filters = null,
    int Page = 1,
    int Limit = 10
)
{
    // Construtor para model binding do ASP.NET Core
    public SearchPatientsQuery() : this(null, 1, 10) { }
};

public record SearchPatientsResponse(
    IReadOnlyCollection<PatientProfileResponse> Data,
    int Total,
    int Page,
    int Limit,
    int TotalPages
);