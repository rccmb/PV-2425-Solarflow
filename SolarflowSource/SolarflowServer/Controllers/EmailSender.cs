using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SolarflowServer.Models;

namespace SolarflowServer.Controllers;

public class EmailSender(IOptions<EmailConfiguration> emailConfig)
{
    private readonly EmailConfiguration _emailConfig = emailConfig.Value;


    public MimeMessage CreateMessage(List<string> recipientEmails, string subject, string body,
        bool isHtml = false)
    {
        var message = new MimeMessage();

        // From
        message.From.Add(new MailboxAddress("Solarflow Project", _emailConfig.From));

        // To
        foreach (var email in recipientEmails) message.To.Add(new MailboxAddress("", email));

        // Subject
        message.Subject = subject;

        // Body
        var bodyBuilder = new BodyBuilder();
        if (isHtml)
            bodyBuilder.HtmlBody = body;
        else
            bodyBuilder.TextBody = body;
        message.Body = bodyBuilder.ToMessageBody();

        return message;
    }


    public async Task SendMessage(MimeMessage mailMessage)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(_emailConfig.Server, _emailConfig.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_emailConfig.Username, _emailConfig.Password);
        await client.SendAsync(mailMessage);
        await client.DisconnectAsync(true);
    }
}