using System.Net;
using System.Net.Mail;
using Application.DTOs.Settings;
using Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;


public class EmailService(IOptions<EmailSettings> options) : IEmailService
{
    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            using var smtpClient = new SmtpClient(options.Value.SmtpServer, options.Value.Port);
            smtpClient.Credentials = new NetworkCredential(options.Value.Email, options.Value.Password);
            smtpClient.EnableSsl = true;

            var mail = new MailMessage(options.Value.Email, to, subject, body);
            mail.IsBodyHtml = true;

            await smtpClient.SendMailAsync(mail);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

}
