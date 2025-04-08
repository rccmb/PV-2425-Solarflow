namespace SolarflowServer.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string userId, string brief, string description, string ipAddress);
    }
}
