using SolarflowServer.Models;

namespace SolarflowServer.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task LogAsync(string userId, string email, string action, string ipAddress)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Email = email,
                Action = action,
                IPAddress = ipAddress
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
