using Mind_Manager.Domain.Entities;

public class Session
    {
        public Guid Id { get; set; }
        public Guid PsychologistId { get; set; }
        public Guid PatientId { get; set; }
        public Guid? AppointmentId { get; set; }
        public string? Complaint { get; set; } // Queixa principal
        public string? Intervention { get; set; }
        public string? Referrals { get; set; } // Encaminhamentos
        public DateTime SessionDate { get; set; } = DateTime.UtcNow;

        public virtual PsychologistProfile Psychologist { get; set; } = null!;
        public virtual PatientProfile Patient { get; set; } = null!;
        public virtual Appointment? Appointment { get; set; }
    }