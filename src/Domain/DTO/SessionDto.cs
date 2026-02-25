using System;

namespace Mind_Manager.src.Domain.DTO;

public record CreateSessionCommand(
    Guid PsychologistId,
    Guid PatientId,
    Guid? AppointmentId = null,
    string? Complaint = null,
    string? Intervention = null,
    string? Referrals = null
);

public record UpdateSessionCommand(
    string? Complaint = null,
    string? Intervention = null,
    string? Referrals = null
);

public record SessionResponse(
    Guid Id,
    Guid PsychologistId,
    Guid PatientId,
    Guid? AppointmentId,
    string? Complaint,
    string? Intervention,
    string? Referrals,
    DateTime SessionDate
);

public record SearchSessionsQuery(
    Guid? Id = null,
    Guid? PsychologistId = null,
    Guid? PatientId = null,
    Guid? AppointmentId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int Limit = 10,
    string SortBy = "SessionDate",
    bool SortDescending = false
);

public record SearchSessionsResponse(
    IReadOnlyCollection<SessionResponse> Sessions,
    int Total,
    int Page,
    int Limit,
    int TotalPages
);