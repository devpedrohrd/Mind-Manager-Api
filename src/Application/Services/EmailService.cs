using MailKit.Net.Smtp;
using MimeKit;

namespace Mind_Manager;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
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

            // Caminho absoluto para index.html na raiz do projeto
            var projectRoot = AppContext.BaseDirectory;
            while (!File.Exists(Path.Combine(projectRoot, "index.html")) && Directory.GetParent(projectRoot) != null)
                projectRoot = Directory.GetParent(projectRoot)!.FullName;
            var htmlTemplatePath = Path.Combine(projectRoot, "index.html");
            var html = await File.ReadAllTextAsync(htmlTemplatePath);
            html = html.Replace("{resetLink}", resetLink);

            message.Body = new TextPart("html")
            {
                Text = html
            };

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para: {Email}", toEmail);
            throw;
        }
    }
}