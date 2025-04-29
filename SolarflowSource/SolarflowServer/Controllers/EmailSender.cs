using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SolarflowServer.Models;

namespace SolarflowServer.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="EmailSender"/> class.
/// </summary>
/// <param name="emailConfig">The email configuration settings.</param>
public class EmailSender(IOptions<EmailConfiguration> emailConfig)
{
    private readonly EmailConfiguration _emailConfig = emailConfig.Value;

    /// <summary>
    /// Creates an email message with the specified recipients, subject, and body.
    /// </summary>
    /// <param name="recipientEmails">A list of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="isHtml">Indicates whether the body content is in HTML format. Defaults to <c>false</c>.</param>
    /// <returns>A <see cref="MimeMessage"/> object representing the email message.</returns>
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

    /// <summary>
    /// Sends an email message using the configured SMTP server.
    /// </summary>
    /// <param name="mailMessage">The <see cref="MimeMessage"/> object representing the email to be sent.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendMessage(MimeMessage mailMessage)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(_emailConfig.Server, _emailConfig.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_emailConfig.Username, _emailConfig.Password);
        await client.SendAsync(mailMessage);
        await client.DisconnectAsync(true);
    }
}