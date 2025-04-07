using SolarflowServer.Models;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }


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
