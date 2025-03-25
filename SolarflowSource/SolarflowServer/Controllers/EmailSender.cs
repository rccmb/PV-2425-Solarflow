using Microsoft.Extensions.Options;
using MimeKit;
using SolarflowServer.Models;
using SolarflowServer.Services;

namespace SolarflowServer.Controllers
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration emailConfig;

        public EmailSender(IOptions<EmailConfiguration> emailConfig)
        {
            this.emailConfig = emailConfig.Value;
        }

        public void SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);

            SendEmail(emailMessage);
        }

        public async Task SendEmailAsync(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            
            await SendAsync(emailMessage);
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(MailboxAddress.Parse(emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = string.Format("<a href=\"{0}\" style=\"color:red; text-decoration:none;\">\r\n    <h2>{0}</h2>\r\n</a>", message.Content)};

            return emailMessage;
        }

        private void SendEmail(MimeMessage mailMessage)
        {
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate(emailConfig.Username, emailConfig.Password);
                client.Send(mailMessage);
                client.Disconnect(true);
            }
        }

        private async Task SendAsync(MimeMessage mailMessage)
        {
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailConfig.Username, emailConfig.Password);
                await client.SendAsync(mailMessage);
                await client.DisconnectAsync(true);
            }

        }
        

    }
}
