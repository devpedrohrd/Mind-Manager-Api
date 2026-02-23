using System.ComponentModel.DataAnnotations;

namespace Mind_Manager.src.Domain.DTO;

public record CreatePsychologistCommand(
    Guid? UserId,
    [MaxLength(100)] string Specialty,
    [MaxLength(20)] string Crp
);

public record UpdatePsychologistCommand(
    [MaxLength(100)] string? Specialty = null,
    [MaxLength(20)] string? Crp = null
);

public record PsychologistResponse(
    Guid Id,
    Guid UserId,
    string Specialty,
    string Crp
);

public record PsychologistFilters(
    Guid? Id = null,
    Guid? UserId = null,
    string? Specialty = null,
    string? Crp = null
);

public record SearchPsychologistsQuery(
    PsychologistFilters Filters,
    int Page = 1,
    int Limit = 10
);

public record SearchPsychologistsResponse(
    IReadOnlyCollection<PsychologistResponse> Data,
    int Total,
    int Page,
    int Limit,
    int TotalPages
);