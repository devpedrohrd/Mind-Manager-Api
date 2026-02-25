using System;
using Microsoft.AspNetCore.Mvc;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager.src.Domain.Interfaces;

public interface IAppointment
{
    Task<ActionResult<Appointment>> CreateAppointmentAsync(Appointment createAppointmentDto);
    Task<bool> UpdateAppointmentAsync(Appointment updateAppointmentDto);
    Task<bool> DeleteAppointmentAsync(Guid appointmentId);
    Task<Appointment?> GetAppointmentByIdAsync(Guid appointmentId);
    Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentsByPsychologistIdAsync(Guid psychologistId);
    Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentsByPatientIdAsync(Guid patientId);
    Task<SearchAppointmentsResponse> GetAppointmentsByFilterAsync(SearchAppointmentsQuery filters);
}
