namespace Mind_Manager.src.Domain.DTO;
using System.ComponentModel.DataAnnotations;

public record CreateAnamnesisCommand(
    Guid PatientId,
    [MaxLength(500), MinLength(1)] string? FamilyHistory = null,
    [MaxLength(500), MinLength(1)] string? Illnesses = null,
    [MaxLength(500), MinLength(1)] string? Infancy = null,
    [MaxLength(500), MinLength(1)] string? Adolescence = null,
    [MaxLength(500), MinLength(1)] string? Accompaniment = null
);

public record UpdateAnamnesisCommand(
    [MaxLength(500), MinLength(1)] string? FamilyHistory = null,
    [MaxLength(500), MinLength(1)] string? Illnesses = null,
    [MaxLength(500), MinLength(1)] string? Infancy = null,
    [MaxLength(500), MinLength(1)] string? Adolescence = null,
    [MaxLength(500), MinLength(1)] string? Accompaniment = null
);

public record AnamnesisResponse(
    Guid Id,
    Guid PatientId,
    string? FamilyHistory,
    string? SocialHistory,
    string? PsychologicalHistory,
    string? Illnesses,
    string? Accompaniment,
    DateTime CreatedAt
);

public record SearchAnamnesisQuery(
    Guid? Id = null,
    Guid? PatientId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);

public record SearchAnamnesisResponse(
    IReadOnlyCollection<AnamnesisResponse> AnamnesisList,
    int Total,
    int Page,
    int Limit,
    int TotalPages
);