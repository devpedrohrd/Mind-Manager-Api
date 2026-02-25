namespace Mind_Manager.Domain.Entities;
using Mind_Manager.Domain.Entities;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.src.Domain.DTO;

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

    public Session(Guid psychologistId, Guid patientId, Guid? appointmentId = null, string? complaint = null, string? intervention = null, string? referrals = null)
    {
        if(psychologistId == Guid.Empty)
            throw new ValidationException("O ID do psicólogo não pode ser vazio.");
        if(patientId == Guid.Empty)
            throw new ValidationException("O ID do paciente não pode ser vazio.");

        Id = Guid.NewGuid();
        PsychologistId = psychologistId;
        PatientId = patientId;
        AppointmentId = appointmentId;
        Complaint = complaint;
        Intervention = intervention;
        Referrals = referrals;
    }

    public void UpdateSession(string? complaint, string? intervention, string? referrals)
    {
        Complaint = complaint;
        Intervention = intervention;
        Referrals = referrals;
    }

    public SessionResponse ToResponseDto()
    {
        return new SessionResponse(
            Id,
            PsychologistId,
            PatientId,
            AppointmentId,
            Complaint,
            Intervention,
            Referrals,
            SessionDate
        );
    }

    public SearchSessionsResponse ToResponseDtoList(IReadOnlyCollection<Session> sessions, int total, int page, int limit)
    {
        var sessionResponses = sessions.Select(s => s.ToResponseDto()).ToList();
        var totalPages = (int)Math.Ceiling((double)total / limit);
        return new SearchSessionsResponse(sessionResponses, total, page, limit, totalPages);
    }
}