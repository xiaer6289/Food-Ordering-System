using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Get your SMTP settings from appsettings.json
        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
        var smtpUser = _configuration["Email:SmtpUser"];
        var smtpPass = _configuration["Email:SmtpPass"];

        using (var client = new SmtpClient(smtpHost, smtpPort))
        {
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(smtpUser, smtpPass);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser, "WSM"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            client.Send(mailMessage);
        }

        return Task.CompletedTask;
    }
}
