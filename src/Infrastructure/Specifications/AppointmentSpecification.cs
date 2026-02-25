using System;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager.src.Infrastructure.Specifications;

public class AppointmentSpecification
{
    public static IQueryable<Appointment> ApplyFilters(IQueryable<Appointment> query, SearchAppointmentsQuery filters)
    {
        var appointmentFilters = filters.Filters ?? new AppointmentFilters();
        
        if(appointmentFilters.Id.HasValue)
            query = query.Where(p => p.Id == appointmentFilters.Id.Value);
        if(appointmentFilters.PatientId.HasValue)
            query = query.Where(p => p.PatientId == appointmentFilters.PatientId.Value);
        if(appointmentFilters.PsychologistId.HasValue)
            query = query.Where(p => p.PsychologistId == appointmentFilters.PsychologistId.Value);
        if(appointmentFilters.Status.HasValue)
            query = query.Where(p => p.Status == appointmentFilters.Status.Value);
        if(appointmentFilters.Type.HasValue)
            query = query.Where(p => p.Type == appointmentFilters.Type.Value);
        if(appointmentFilters.ActivityType.HasValue)
            query = query.Where(p => p.ActivityType == appointmentFilters.ActivityType.Value);
        if(appointmentFilters.StartDate.HasValue)
            query = query.Where(p => p.AppointmentDate.Date >= appointmentFilters.StartDate.Value.Date);
        if(appointmentFilters.EndDate.HasValue)
            query = query.Where(p => p.AppointmentDate.Date <= appointmentFilters.EndDate.Value.Date);

        return query;
    }

    public static IQueryable<Appointment> ApplySorting(IQueryable<Appointment> query, SearchAppointmentsQuery filters)
    {
        var sortBy = filters.SortBy?.ToLower() switch
        {
            "createddate" or "createdat" => filters.SortDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
            "status" => filters.SortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            "appointmentdate" or _ => filters.SortDescending ? query.OrderByDescending(a => a.AppointmentDate) : query.OrderBy(a => a.AppointmentDate)
        };
        
        // Adicionar ordenação secundária para consistência
        return sortBy.ThenBy(a => a.Id);
    }

    public static IQueryable<Appointment> ApplyPagination(IQueryable<Appointment> query, int page, int limit)
    {
        return query
            .Skip((page - 1) * limit)
            .Take(limit);
    }

}
