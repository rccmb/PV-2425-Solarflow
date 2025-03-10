namespace SolarflowServer.Services
{
    public interface IAuditService
    {
        Task LogAsync(string userId, string email, string action, string ipAddress);
    }
}
