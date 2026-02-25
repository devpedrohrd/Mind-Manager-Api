using Mind_Manager.Domain.Entities;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.src.Domain.DTO;
using Mind_Manager.src.Domain.Interfaces;
using ISession = Mind_Manager.src.Domain.Interfaces.ISession;

namespace Mind_Manager.src.Application.Services;

public interface ISessionService
{
    Task<SessionResponse> CreateSessionAsync(CreateSessionCommand createSessionDto);
    Task<SessionResponse?> GetSessionByIdAsync(Guid sessionId, Guid userIdRequesting, bool isPsychologist);
    Task<SearchSessionsResponse> GetSessionsByFilterAsync(SearchSessionsQuery filters, Guid userIdRequesting, bool isPsychologist);
    Task<bool> UpdateSessionAsync(Guid sessionId, UpdateSessionCommand updateSessionDto, Guid userIdRequesting, bool isPsychologist);
    Task<bool> DeleteSessionAsync(Guid sessionId, Guid userIdRequesting, bool isPsychologist);
}
public class SessionService(ISession sessionRepository, IPsychologistService psychologistService, IPatientService patientService, IAppointmentService appointmentService) : ISessionService
{
    private readonly ISession _sessionRepository = sessionRepository;
    private readonly IPsychologistService _psychologistService = psychologistService;
    private readonly IPatientService _patientService = patientService;
    private readonly IAppointmentService _appointmentService = appointmentService;
    public async Task<SessionResponse> CreateSessionAsync(CreateSessionCommand createSessionDto)
    {
        var psychologist = await _psychologistService.GetByIdAsync(createSessionDto.PsychologistId);
        var patientExists = await _patientService.GetByIdAsync(createSessionDto.PatientId) != null;
        if (psychologist == null || !patientExists)
            throw new NotFoundException("Psychologist or patient not found.");
        var appointment = null as AppointmentResponse;
        if (createSessionDto.AppointmentId.HasValue)
        {
            appointment = await _appointmentService.GetAppointmentByIdAsync(createSessionDto.AppointmentId.Value, psychologist.UserId, UserRole.Psychologist.ToString()) ?? throw new NotFoundException("Appointment not found.");
        }

        var session = new Session(
            psychologistId: createSessionDto.PsychologistId,
            patientId: createSessionDto.PatientId,
            appointmentId: createSessionDto.AppointmentId ?? appointment?.Id,
            complaint: createSessionDto.Complaint,
            intervention: createSessionDto.Intervention,
            referrals: createSessionDto.Referrals
        );

        await _sessionRepository.CreateSessionAsync(session);
        return session.ToResponseDto();
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId, Guid userIdRequesting, bool isPsychologist)
    {
        var session = await _sessionRepository.GetSessionByIdAsync(sessionId) ?? throw new NotFoundException("Session not found.");

        await ValidateSessionAccessAsync(session, userIdRequesting, isPsychologist);

        var result = await _sessionRepository.DeleteSessionAsync(sessionId);
        if (result)
            return true;
        return result;
    }

    public async Task<SessionResponse?> GetSessionByIdAsync(Guid sessionId, Guid userIdRequesting, bool isPsychologist)
    {
        var session = await _sessionRepository.GetSessionByIdAsync(sessionId);

        await ValidateSessionAccessAsync(session ?? throw new NotFoundException("Session not found."), userIdRequesting, isPsychologist);
        return session?.ToResponseDto();
    }

    public async Task<SearchSessionsResponse> GetSessionsByFilterAsync(SearchSessionsQuery filters, Guid userIdRequesting, bool isPsychologist)
    {
        if (isPsychologist)
        {
            var psychologist = await _psychologistService.GetByUserIdAsync(userIdRequesting)
                ?? throw new UnauthorizedAccessException("Psychologist profile not found.");

            filters = filters with { PsychologistId = psychologist.Id };
        }
        else
        {
            var patient = await _patientService.GetByUserIdAsync(userIdRequesting)
                ?? throw new UnauthorizedAccessException("Patient profile not found.");

            filters = filters with { PatientId = patient.Id };
        }

        return await _sessionRepository.GetSessionsByFilterAsync(filters);
    }

    public async Task<bool> UpdateSessionAsync(Guid sessionId, UpdateSessionCommand updateSessionDto, Guid userIdRequesting, bool isPsychologist)
    {
        var session = await _sessionRepository.GetSessionByIdAsync(sessionId) ?? throw new NotFoundException("Session not found.");

        await ValidateSessionAccessAsync(session, userIdRequesting, isPsychologist);

        session.UpdateSession(updateSessionDto.Complaint ?? session.Complaint, updateSessionDto.Intervention ?? session.Intervention, updateSessionDto.Referrals ?? session.Referrals);
        await _sessionRepository.UpdateSessionAsync(session);
        return true;
    }

    private async Task ValidateSessionAccessAsync(Session session, Guid userIdRequesting, bool isPsychologist)
    {
        if (isPsychologist)
        {
            var psychologistProfile = await _psychologistService.GetByUserIdAsync(userIdRequesting);
            if (psychologistProfile == null || session.PsychologistId != psychologistProfile.Id)
                throw new UnauthorizedException("You don't have permission to access this session.");
        }
        else
        {
            var patientProfile = await _patientService.GetByUserIdAsync(userIdRequesting);
            if (patientProfile == null || session.PatientId != patientProfile.Id)
                throw new UnauthorizedException("You don't have permission to access this session.");
        }
    }
}