using System;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager.src.Application.Mappers;

public static class AppointmentMapper
{
        public static AppointmentResponse ToResponseDto(Appointment a)
        {
            return new AppointmentResponse(
                Id: a.Id,
                PatientId: a.PatientId,
                PsychologistId: a.PsychologistId,
                Status: a.Status,
                Type: a.Type,
                AppointmentDate: a.AppointmentDate,
                ActivityType: a.ActivityType,
                Reason: a.Reason,
                Observation: a.Observation,
                Objective: a.Objective,
                CreatedAt: a.CreatedAt
            );
        }

        public static IEnumerable<AppointmentResponse> ToResponseDtoList(IEnumerable<Appointment> appointments)
        {
            return appointments.Select(ToResponseDto);
        }

        public static UpdateAppointmentCommand ToDomainModel(UpdateAppointmentCommand a)
        {
            return new UpdateAppointmentCommand(
                AppointmentDate: a.AppointmentDate ?? DateTime.MinValue,
                Status: a.Status ?? default,
                Type: a.Type,
                ActivityType: a.ActivityType,
                Reason: a.Reason,
                Observation: a.Observation,
                Objective: a.Objective
            );
        }
}
