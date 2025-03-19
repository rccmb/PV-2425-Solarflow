namespace SolarflowServer
{
    public interface IEmailSender
    {
        void SendEmail(Message message);
    }
}
