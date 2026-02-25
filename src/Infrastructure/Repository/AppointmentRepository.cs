using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mind_Manager.Infrastructure.Persistence;
using Mind_Manager.src.Domain.DTO;
using Mind_Manager.src.Domain.Interfaces;
using Mind_Manager.src.Infrastructure.Specifications;
using Mind_Manager.src.Application.Mappers;

namespace Mind_Manager.src.Infrastructure.Repository;

public class AppointmentRepository : IAppointment
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ActionResult<Appointment>> CreateAppointmentAsync(Appointment createAppointmentDto)
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

    public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentsByPatientIdAsync(Guid patientId)
    {
        var appointments = await _context.Appointments.Where(a => a.PatientId == patientId).ToListAsync();
        return appointments;
    }

    public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentsByPsychologistIdAsync(Guid psychologistId)
    {
        var appointments = await _context.Appointments.Where(a => a.PsychologistId == psychologistId).ToListAsync();
        return appointments;
    }

    public Task<bool> UpdateAppointmentAsync(Appointment updateAppointmentDto)
    {
        _context.Appointments.Update(updateAppointmentDto);
        return Task.FromResult(true);
    }
}
