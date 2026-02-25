using System;
using System.ComponentModel.DataAnnotations;

namespace Mind_Manager.src.Domain.DTO;

public record CreateAppointmentCommand(
    Guid? PsychologistId,
    Guid PatientId,
    Status Status = Status.Pending,
    TypeAppointment? Type = null,
    DateTime AppointmentDate = default,
    ActivityType? ActivityType = null,
    [MaxLength(255), MinLength(1)] string? Reason = null,
    [MaxLength(255), MinLength(1)] string? Observation = null,
    [MaxLength(255), MinLength(1)] string? Objective = null
);

public record UpdateAppointmentCommand(
    Status? Status = null,
    TypeAppointment? Type = null,
    DateTime? AppointmentDate = null,
    ActivityType? ActivityType = null,
    [MaxLength(255), MinLength(1)] string? Reason = null,
    [MaxLength(255), MinLength(1)] string? Observation = null,
    [MaxLength(255), MinLength(1)] string? Objective = null
);

public record AppointmentResponse(
    Guid Id,
    Guid PsychologistId,
    Guid? PatientId,
    Status Status,
    TypeAppointment? Type,
    DateTime AppointmentDate,
    ActivityType? ActivityType,
    string? Reason,
    string? Observation,
    string? Objective,
    DateTime CreatedAt
);

public record AppointmentFilters(
    Guid? Id = null,
    Guid? PsychologistId = null,
    Guid? PatientId = null,
    Status? Status = null,
    TypeAppointment? Type = null,
    ActivityType? ActivityType = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);

public record SearchAppointmentsQuery(
    AppointmentFilters? Filters = null,
    int Page = 1,
    int Limit = 10,
    string SortBy = "AppointmentDate",
    bool SortDescending = false
)
{
    public SearchAppointmentsQuery() : this(null, 1, 10) { }
};

public record SearchAppointmentsResponse(
    IReadOnlyCollection<AppointmentResponse> Data,
    int TotalCount,
    int Page,
    int Limit,
    int TotalPages
);