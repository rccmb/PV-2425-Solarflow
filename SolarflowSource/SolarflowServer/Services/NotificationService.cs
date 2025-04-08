using SolarflowServer.DTOs.Notification;
using SolarflowServer.Models;
using SolarflowServer.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace SolarflowServer.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.TimeSent)
                .ToListAsync();

            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Description = n.Description,
                TimeSent = n.TimeSent,
                TimeRead = n.TimeRead,
                Status = n.Status.ToString()
            });
        }

        public async Task<NotificationDto> GetNotificationByIdAsync(int id, int userId)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null || notification.UserId != userId)
                return null;

            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Description = notification.Description,
                TimeSent = notification.TimeSent,
                TimeRead = notification.TimeRead,
                Status = notification.Status.ToString()
            };
        }

        public async Task CreateNotificationAsync(int userId, NotificationCreateDto dto)
        {
            var notification = new Notification
            {
                Title = dto.Title,
                Description = dto.Description,
                UserId = userId,
                TimeSent = DateTime.UtcNow,
                Status = NotificationStatus.Unread
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int id, int userId)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null || notification.UserId != userId) return;

            notification.MarkAsRead();
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNotificationAsync(int id, int userId)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null || notification.UserId != userId) return;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllNotificationsAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
        }
    }
}
