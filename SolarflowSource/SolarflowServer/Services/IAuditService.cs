namespace SolarflowServer.Services
{
    public interface IAuditService
    {
        Task LogAsync(string userId, string brief, string description, string ipAddress);
    }
}
