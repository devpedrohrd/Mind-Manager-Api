using Mind_Manager.Domain.Entities;

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
    }