using Microsoft.EntityFrameworkCore;
using Mind_Manager.Infrastructure.Persistence;
using Mind_Manager.src.Domain.Interfaces;

namespace Mind_Manager.src.Infrastructure.Repository;

public class EmailScheduleRepository(ApplicationDbContext context) : IEmailSchedule
{
    private readonly ApplicationDbContext _context = context;

    public async Task<EmailSchedule?> FindByAppointmentIdAsync(Guid appointmentId)
    {
        return await _context.EmailSchedules
            .FirstOrDefaultAsync(e => e.AppointmentId == appointmentId && e.IsSent);
    }

    public async Task CreateAsync(EmailSchedule emailSchedule)
    {
        await _context.EmailSchedules.AddAsync(emailSchedule);
    }

    public async Task DeleteByAppointmentIdAsync(Guid appointmentId)
    {
        var schedules = await _context.EmailSchedules
            .Where(e => e.AppointmentId == appointmentId)
            .ToListAsync();

        if (schedules.Count > 0)
            _context.EmailSchedules.RemoveRange(schedules);
    }
}
