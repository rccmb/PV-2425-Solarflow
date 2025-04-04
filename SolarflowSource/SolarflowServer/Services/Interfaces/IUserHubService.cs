namespace SolarflowServer.Services.Interfaces;

public interface IUserHubService
{
    Task<bool> UserOwnsHubAsync(int userId, int hubId);
}