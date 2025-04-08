using SolarflowServer.DTOs.Notification;
using SolarflowServer.Models;
using SolarflowServer.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace SolarflowServer.Services
{
    // Service responsible for business logic related to notifications
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        // Inject the database context
        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retrieves all notifications for a specific user, sorted by most recent
        public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.TimeSent)
                .ToListAsync();

            // Maps Notification entities to DTOs for response
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

        // Retrieves a single notification by ID, ensuring it belongs to the specified user
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

        // Creates a new notification for the specified user
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

        // Marks a specific notification as read if it belongs to the user
        public async Task MarkAsReadAsync(int id, int userId)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null || notification.UserId != userId) return;

            notification.MarkAsRead();
            await _context.SaveChangesAsync();
        }

        // Deletes a specific notification if it belongs to the user
        public async Task DeleteNotificationAsync(int id, int userId)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null || notification.UserId != userId) return;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }

        // Deletes all notifications for a specific user
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
