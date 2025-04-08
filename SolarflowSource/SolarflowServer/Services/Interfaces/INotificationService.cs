using System.Collections.Generic;
using System.Threading.Tasks;
using SolarflowServer.DTOs.Notification;

namespace SolarflowServer.Services
{
    // Defines the contract for all notification-related operations
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int userId);
        Task<NotificationDto> GetNotificationByIdAsync(int id, int userId);
        Task CreateNotificationAsync(int userId, NotificationCreateDto dto);
        Task MarkAsReadAsync(int id, int userId);
        Task DeleteNotificationAsync(int id, int userId);
        Task DeleteAllNotificationsAsync(int userId);
    }
}