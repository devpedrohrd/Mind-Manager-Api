using Mind_Manager.Domain.Entities;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.src.Domain.DTO;

public class Anamnesis
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string? FamilyHistory { get; set; }
    public string? Infancy { get; set; }
    public string? Adolescence { get; set; }
    public string? Illnesses { get; set; }
    public string? Accompaniment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual PatientProfile Patient { get; set; } = null!;

    public Anamnesis(Guid patientId, string? familyHistory = null, string? infancy = null, string? adolescence = null, string? illnesses = null, string? accompaniment = null)
    {
        if (patientId == Guid.Empty)
            throw new ValidationException("O ID do paciente não pode ser vazio.");

        Id = Guid.NewGuid();
        PatientId = patientId;
        FamilyHistory = familyHistory;
        Infancy = infancy;
        Adolescence = adolescence;
        Illnesses = illnesses;
        Accompaniment = accompaniment;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateAnamnesis(string? familyHistory, string? infancy, string? adolescence, string? illnesses, string? accompaniment)
    {
        FamilyHistory = familyHistory;
        Infancy = infancy;
        Adolescence = adolescence;
        Illnesses = illnesses;
        Accompaniment = accompaniment;
    }

    public AnamnesisResponse ToResponseDto()
    {
        return new AnamnesisResponse(
            Id,
            PatientId,
            FamilyHistory,
            Infancy,
            Adolescence,
            Illnesses,
            Accompaniment,
            CreatedAt
        );
    }
}