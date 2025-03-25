using SolarflowServer.Models;

namespace SolarflowServer.Services
{
    public interface IEmailSender
    {
        void SendEmail(Message message);

        Task SendEmailAsync(Message message);
    }
}
