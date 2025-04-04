namespace SolarflowServer.Services;

public class UserHubService : IUserHubService
{
    private readonly ApplicationDbContext _context;

    public UserHubService(ApplicationDbContext context)
    {
        _context = context;
    }

    // This method checks if the user owns a given hub.
    public async Task<bool> UserOwnsHubAsync(int userId, int hubId)
    {
        // Assuming you have a UserHub model that links users to hubs (user-hub relationship)
        return await _context.UserHubs
            .AnyAsync(uh => uh.UserId == userId && uh.HubId == hubId);
    }
}