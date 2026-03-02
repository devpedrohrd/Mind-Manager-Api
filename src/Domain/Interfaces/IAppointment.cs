using System;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager.src.Domain.Interfaces;

public interface IAppointment
{
    Task<Appointment> CreateAppointmentAsync(Appointment createAppointmentDto);
    Task<bool> UpdateAppointmentAsync(Appointment updateAppointmentDto);
    Task<bool> DeleteAppointmentAsync(Guid appointmentId);
    Task<Appointment?> GetAppointmentByIdAsync(Guid appointmentId);
    Task<IEnumerable<Appointment>> GetAppointmentsByPsychologistIdAsync(Guid psychologistId);
    Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(Guid patientId);
    Task<SearchAppointmentsResponse> GetAppointmentsByFilterAsync(SearchAppointmentsQuery filters);
    Task<AppointmentsPendingsResponse> GetPendingAppointmentsForPsychologistAsync(DateTime? startDate, DateTime? endDate, Guid userIdRequesting);
    Task<List<Appointment>> GetTodayPendingAppointmentsWithDetailsAsync();
}
