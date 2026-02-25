using Mind_Manager.Domain.Entities;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.src.Domain.DTO;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid PsychologistId { get; set; }
    public Guid PatientId { get; set; }
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
        Guid patientId,
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
        if(patientId == Guid.Empty)
            throw new ValidationException("O ID do paciente não pode ser vazio.");

        Id = Guid.NewGuid();
        PsychologistId = psychologistId;
        PatientId = patientId;
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

    public void UpdateStatus(Status newStatus)
    {
        if (Status == Status.Canceled)
            throw new BusinessException("Não é possível alterar o status de um agendamento cancelado.");

        Status = newStatus;
    }

    public void AssignPatient(Guid patientId)
    {
        if (patientId == Guid.Empty)
            throw new ValidationException("O ID do paciente não pode ser vazio.");

        if (Status == Status.Canceled)
            throw new BusinessException("Não é possível atribuir um paciente a um agendamento cancelado.");

        PatientId = patientId;
    }

    public void Cancel()
    {
        if (Status == Status.Canceled)
            throw new BusinessException("O agendamento já está cancelado.");

        Status = Status.Canceled;
    }

    public void Reschedule(DateTime newDate)
    {
        if (Status == Status.Canceled)
            throw new BusinessException("Não é possível reagendar um agendamento cancelado.");

        AppointmentDate = newDate;
    }

    public void AddObservation(string observation)
    {
        if (string.IsNullOrWhiteSpace(observation))
            throw new ValidationException("A observação não pode ser vazia.");

        Observation = observation;
    }

    public void AddObjective(string objective)
    {
        if (string.IsNullOrWhiteSpace(objective))
            throw new ValidationException("O objetivo não pode ser vazio.");

        Objective = objective;
    }


    public void UpdateType(TypeAppointment type)
    {
        if (Status == Status.Canceled)
            throw new BusinessException("Não é possível alterar o tipo de um agendamento cancelado.");

        Type = type;
    }

    public void UpdateActivityType(ActivityType activityType)
    {
        if (Status == Status.Canceled)
            throw new BusinessException("Não é possível alterar o tipo de atividade de um agendamento cancelado.");

        ActivityType = activityType;
    }

    public void RemovePatient()
    {
        if (Status == Status.Canceled)
            throw new BusinessException("Não é possível remover o paciente de um agendamento cancelado.");

        PatientId = Guid.Empty;
    }

    public void RemoveObservation()
    {
        if (Status == Status.Canceled)
            throw new BusinessException("Não é possível remover a observação de um agendamento cancelado.");

        Observation = null;
    }

    public void RemoveObjective()
    {
        if (Status == Status.Canceled)
            throw new BusinessException("Não é possível remover o objetivo de um agendamento cancelado.");

        Objective = null;
    }
}