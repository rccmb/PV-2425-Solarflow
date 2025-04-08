using SolarflowServer.Models;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Services
{
    /// <summary>
    /// Service for logging audit information related to user actions.
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditService"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Logs an action performed by a user.
        /// </summary>
        /// <param name="userId">The ID of the user who performed the action.</param>
        /// <param name="brief">A brief description of the action.</param>
        /// <param name="action">The action performed by the user.</param>
        /// <param name="ipAddress">The IP address from which the action was performed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogAsync(string userId, string brief, string action, string ipAddress)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Brief = brief,
                Action = action,
                IPAddress = ipAddress
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
