using Mind_Manager.Domain.Entities;

public class Appointment
    {
        public Guid Id { get; set; }
        public Guid PsychologistId { get; set; }
        public Guid? PatientId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public Status Status { get; set; } = Status.Pending;
        public TypeAppointment? Type { get; set; }
        public ActivityType? ActivityType { get; set; }
        public string? Reason { get; set; }
        public string? Observation { get; set; }
        public string? Objective { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual PsychologistProfile Psychologist { get; set; } = null!;
        public virtual PatientProfile? Patient { get; set; }
        public virtual Session? Session { get; set; }
        public virtual ICollection<EmailSchedule> EmailSchedules { get; set; } = [];
    }