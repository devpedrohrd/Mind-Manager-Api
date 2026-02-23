public class EmailSchedule
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public DateTime SendAt { get; set; }
        public bool IsSent { get; set; } = false;

        public virtual Appointment Appointment { get; set; } = null!;
    }