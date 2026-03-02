using Mind_Manager.Domain.Interfaces;

namespace Mind_Manager.src.Application.Services;

public class EmailReminderBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<EmailReminderBackgroundService> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<EmailReminderBackgroundService> _logger = logger;
    private readonly TimeOnly _targetTime = new(6, 0); // 6:00 AM

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailReminderBackgroundService iniciado. Agendado para rodar diariamente às {Time}.", _targetTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextRun();
            _logger.LogInformation("Próxima verificação de emails em {Delay} (às {NextRun}).",
                delay, DateTime.Now.Add(delay).ToString("dd/MM/yyyy HH:mm"));

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("EmailReminderBackgroundService cancelado durante espera.");
                break;
            }

            await CheckAndSendRemindersAsync(stoppingToken);
        }

        _logger.LogInformation("EmailReminderBackgroundService encerrado.");
    }

    private TimeSpan CalculateDelayUntilNextRun()
    {
        var now = DateTime.Now;
        var todayTarget = now.Date.Add(_targetTime.ToTimeSpan());

        // Se já passou do horário alvo de hoje, agenda para amanhã
        var nextRun = now < todayTarget ? todayTarget : todayTarget.AddDays(1);

        return nextRun - now;
    }

    private async Task CheckAndSendRemindersAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando verificação de agendamentos para envio de lembretes...");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Busca agendamentos do dia com status Pending ou Confirmed
            var appointments = await unitOfWork.Appointments.GetTodayPendingAppointmentsWithDetailsAsync();

            _logger.LogInformation("Encontrados {Count} agendamento(s) pendentes/confirmados para hoje.", appointments.Count);

            var emailsSent = 0;
            var emailsSkipped = 0;

            foreach (var appointment in appointments)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                var patient = appointment.Patient;
                if (patient?.User == null)
                {
                    _logger.LogWarning("Agendamento {Id} sem paciente ou usuário vinculado. Pulando.", appointment.Id);
                    continue;
                }

                var patientEmail = patient.User.Email;
                var patientName = patient.User.Name;
                var psychologistName = appointment.Psychologist?.User?.Name ?? "Profissional";

                // Verifica se já foi enviado email para este agendamento
                var alreadySent = await unitOfWork.EmailSchedules.FindByAppointmentIdAsync(appointment.Id);

                if (alreadySent != null)
                {
                    _logger.LogDebug("Email já enviado para agendamento {Id}. Pulando.", appointment.Id);
                    emailsSkipped++;
                    continue;
                }

                try
                {
                    await emailService.SendAppointmentReminderAsync(
                        patientEmail,
                        patientName,
                        appointment.AppointmentDate,
                        psychologistName,
                        appointment.Status);

                    // Registra o envio na tabela EmailSchedule
                    var emailSchedule = new EmailSchedule
                    {
                        Id = Guid.NewGuid(),
                        AppointmentId = appointment.Id,
                        SendAt = DateTime.UtcNow,
                        IsSent = true
                    };

                    await unitOfWork.EmailSchedules.CreateAsync(emailSchedule);
                    await unitOfWork.SaveChangesAsync();

                    emailsSent++;
                    _logger.LogInformation("Lembrete enviado para {Email} (Agendamento: {Id}).", patientEmail, appointment.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao enviar lembrete para {Email} (Agendamento: {Id}).", patientEmail, appointment.Id);
                }
            }

            _logger.LogInformation("Verificação concluída. Emails enviados: {Sent}, Já enviados: {Skipped}.", emailsSent, emailsSkipped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro crítico durante verificação de lembretes de email.");
        }
    }
}
