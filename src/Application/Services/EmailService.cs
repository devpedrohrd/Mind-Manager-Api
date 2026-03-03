using MailKit.Net.Smtp;
using MimeKit;

namespace Mind_Manager;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    Task SendEmailToConfirmAppointmentAsync(string toEmail, string appointmentDetails);
    Task SendAppointmentReminderAsync(string toEmail, string patientName, DateTime appointmentDate, string psychologistName, Status status);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _smtpServer = _configuration["SMTP:Server"] ?? "smtp.gmail.com";
        _smtpPort = int.TryParse(_configuration["SMTP:Port"], out var port) ? port : 587;
        _smtpUser = _configuration["SMTP:Username"] ?? "user@example.com";
        _smtpPass = _configuration["SMTP:Password"] ?? "password";

        _logger.LogInformation("EmailService configurado - Server: {Server}, Port: {Port}, User: {User}",
            _smtpServer, _smtpPort, _smtpUser);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Mind Manager", _smtpUser));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Redefinição de senha";

            var projectRoot = FindProjectRoot();
            var htmlTemplatePath = Path.Combine(projectRoot, "index.html");
            var html = await File.ReadAllTextAsync(htmlTemplatePath);
            html = html.Replace("{resetLink}", resetLink);

            message.Body = new TextPart("html") { Text = html };

            await SendEmailAsync(message, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para: {Email}", toEmail);
            throw;
        }
    }

    public async Task SendEmailToConfirmAppointmentAsync(string toEmail, string appointmentDetails)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Mind Manager", _smtpUser));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Confirmação de Agendamento";
            message.Body = new TextPart("plain")
            {
                Text = $"Olá,\n\nSeu agendamento foi confirmado com os seguintes detalhes:\n{appointmentDetails}\n\nAtenciosamente,\nEquipe Mind Manager"
            };

            await SendEmailAsync(message, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para: {Email}", toEmail);
            throw;
        }
    }

    public async Task SendAppointmentReminderAsync(string toEmail, string patientName, DateTime appointmentDate, string psychologistName, Status status)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Mind Manager", _smtpUser));
            message.To.Add(new MailboxAddress(patientName, toEmail));
            message.Subject = "🗓️ Lembrete de Consulta - Mind Manager";

            var projectRoot = FindProjectRoot();
            var htmlTemplatePath = Path.Combine(projectRoot, "appointment-reminder.html");
            var html = await File.ReadAllTextAsync(htmlTemplatePath);

            var (statusText, statusColor) = status switch
            {
                Status.Confirmed => ("Confirmado", "#28a745"),
                Status.Scheduled => ("Agendado", "#ffc107"),
                _ => (status.ToString(), "#6c757d")
            };

            html = html.Replace("{patientName}", patientName)
                       .Replace("{appointmentDate}", appointmentDate.ToString("dd/MM/yyyy"))
                       .Replace("{appointmentTime}", appointmentDate.ToString("HH:mm"))
                       .Replace("{psychologistName}", psychologistName)
                       .Replace("{statusText}", statusText)
                       .Replace("{statusColor}", statusColor);

            message.Body = new TextPart("html") { Text = html };

            await SendEmailAsync(message, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email de lembrete para: {Email}", toEmail);
            throw;
        }
    }

    private async Task SendEmailAsync(MimeMessage message, string toEmail)
    {
        using var client = new SmtpClient();
        _logger.LogInformation("Conectando ao servidor SMTP...");
        await client.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        _logger.LogInformation("Autenticando com usuário: {User}", _smtpUser);
        await client.AuthenticateAsync(_smtpUser, _smtpPass);
        _logger.LogInformation("Enviando email...");
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
        _logger.LogInformation("Email enviado com sucesso para: {Email}", toEmail);
    }

    private static string FindProjectRoot()
    {
        var projectRoot = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(projectRoot, "index.html")) && Directory.GetParent(projectRoot) != null)
            projectRoot = Directory.GetParent(projectRoot)!.FullName;
        return projectRoot;
    }
}