using Microsoft.Extensions.Options;
using MimeKit;

namespace SolarflowServer
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

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(MailboxAddress.Parse(emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Content };

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

    }
}
