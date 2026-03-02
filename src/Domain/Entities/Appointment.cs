using Mind_Manager.Domain.Entities;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.src.Domain.DTO;

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

    public Appointment(
        Guid psychologistId,
        Guid? patientId,
        DateTime appointmentDate,
        Status status = Status.Pending,
        TypeAppointment? type = null,
        ActivityType? activityType = null,
        string? reason = null,
        string? observation = null,
        string? objective = null)
    {
        if (psychologistId == Guid.Empty)
            throw new ValidationException("O ID do psicólogo não pode ser vazio.");

        if (type == TypeAppointment.Session && (patientId == null || patientId == Guid.Empty))
            throw new ValidationException("O ID do paciente é obrigatório para agendamentos do tipo Sessão.");

        Id = Guid.NewGuid();
        PsychologistId = psychologistId;
        PatientId = type == TypeAppointment.Session ? patientId : null;
        AppointmentDate = appointmentDate;
        Status = status;
        Type = type;
        ActivityType = activityType;
        Reason = reason;
        Observation = observation;
        Objective = objective;
    }

    public void ToUpdateCommand(
        Status? status,
        TypeAppointment? type,
        DateTime? appointmentDate,
        ActivityType? activityType,
        string? reason,
        string? observation,
        string? objective
    )
    {
        Status = status ?? Status;
        Type = type ?? Type;
        AppointmentDate = appointmentDate ?? AppointmentDate;
        ActivityType = activityType ?? ActivityType;
        Reason = reason ?? Reason;
        Observation = observation ?? Observation;
        Objective = objective ?? Objective;
    }


}