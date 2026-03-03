using System;
using Microsoft.EntityFrameworkCore;
using Mind_Manager.Infrastructure.Persistence;
using Mind_Manager.src.Domain.DTO;
using Mind_Manager.src.Domain.Interfaces;
using Mind_Manager.src.Infrastructure.Specifications;
using Mind_Manager.src.Application.Mappers;

namespace Mind_Manager.src.Infrastructure.Repository;

public class AppointmentRepository(ApplicationDbContext context) : IAppointment
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Appointment> CreateAppointmentAsync(Appointment createAppointmentDto)
    {
        await _context.Appointments.AddAsync(createAppointmentDto);
        return createAppointmentDto;
    }

    public async Task<bool> DeleteAppointmentAsync(Guid appointmentId)
    {
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
        if (appointment == null)
            return false;
        _context.Appointments.Remove(appointment);
        return true;
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(Guid appointmentId)
    {
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
        if (appointment == null)
            return null;
        return appointment;
    }

    public async Task<SearchAppointmentsResponse> GetAppointmentsByFilterAsync(SearchAppointmentsQuery filters)
    {
        var query = _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Psychologist)
            .AsQueryable();

        var safeFilters = filters ?? new SearchAppointmentsQuery();
        query = AppointmentSpecification.ApplyFilters(query, safeFilters);

        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)safeFilters.Limit);

        query = AppointmentSpecification.ApplySorting(query, safeFilters);
        query = AppointmentSpecification.ApplyPagination(query, safeFilters.Page, safeFilters.Limit);

        var appointments = await query.ToListAsync();

        var response = new SearchAppointmentsResponse(
            Data: [.. appointments.Select(AppointmentMapper.ToResponseDto)],
            TotalCount: total,
            Page: safeFilters.Page,
            Limit: safeFilters.Limit,
            TotalPages: totalPages
        );
        return response;
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(Guid patientId)
    {
        return await _context.Appointments.Where(a => a.PatientId == patientId).ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByPsychologistIdAsync(Guid psychologistId)
    {
        return await _context.Appointments.Where(a => a.PsychologistId == psychologistId).ToListAsync();
    }

    public async Task<AppointmentsPendingsResponse> GetPendingAppointmentsForPsychologistAsync(DateTime? startDate, DateTime? endDate, Guid userIdRequesting)
    {
        var tomorrow = DateTime.Today.AddDays(1);
        
        var pendingEmails = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.EmailSchedules)
            .Where(a => a.AppointmentDate <= DateTime.Now &&
                a.AppointmentDate < tomorrow &&
                (a.Status == Status.Scheduled || a.Status == Status.Confirmed))
            .ToListAsync();

        var response = new AppointmentsPendingsResponse(
            Data: [.. pendingEmails.Select(AppointmentMapper.ToResponseDto)]
        );
        return response;
    }

    public Task<bool> UpdateAppointmentAsync(Appointment updateAppointmentDto)
    {
        _context.Appointments.Update(updateAppointmentDto);
        return Task.FromResult(true);
    }

    public async Task<List<Appointment>> GetTodayPendingAppointmentsWithDetailsAsync()
    {
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);

        return await _context.Appointments
            .Include(a => a.Patient!)
                .ThenInclude(p => p.User)
            .Include(a => a.Psychologist)
                .ThenInclude(p => p.User)
            .Include(a => a.EmailSchedules)
            .Where(a => a.AppointmentDate >= todayUtc
                && a.AppointmentDate < tomorrowUtc
                && (a.Status == Status.Scheduled || a.Status == Status.Confirmed)
                && a.Patient != null
                && a.Patient.User.IsActive)
            .ToListAsync();
    }
}
