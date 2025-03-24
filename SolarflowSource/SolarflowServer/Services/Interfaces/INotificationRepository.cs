using System.Collections.Generic;
using System.Threading.Tasks;
using SolarflowServer.Models;

namespace SolarflowServer.Services
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
        Task<Notification> GetByIdAsync(int id);
        Task AddAsync(Notification notification);
        Task MarkAsReadAsync(int id);
        Task DeleteAsync(int id);
        Task DeleteAllAsync(int userId);
        Task SaveChangesAsync();
    }
}