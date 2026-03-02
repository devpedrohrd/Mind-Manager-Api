namespace Mind_Manager.src.Domain.Interfaces;

public interface IEmailSchedule
{
    Task<EmailSchedule?> FindByAppointmentIdAsync(Guid appointmentId);
    Task CreateAsync(EmailSchedule emailSchedule);
    Task DeleteByAppointmentIdAsync(Guid appointmentId);
}
