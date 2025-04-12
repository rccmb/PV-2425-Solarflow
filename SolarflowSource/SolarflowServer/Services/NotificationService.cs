using SolarflowServer.DTOs.Notification;
using SolarflowServer.Models;
using SolarflowServer.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace SolarflowServer.Services
{
    /// <summary>
    /// Service responsible for business logic related to notifications.
    /// Handles CRUD operations for notifications including retrieval, creation, and deletion.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class.
        /// </summary>
        /// <param name="context">The <see cref="ApplicationDbContext"/> used to interact with the database.</param>
        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all notifications for a specific user, sorted by most recent.
        /// </summary>
        /// <param name="userId">The ID of the user to fetch notifications for.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="NotificationDto"/>.</returns>
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

        /// <summary>
        /// Retrieves a single notification by ID, ensuring it belongs to the specified user.
        /// </summary>
        /// <param name="id">The ID of the notification to retrieve.</param>
        /// <param name="userId">The ID of the user to verify the notification belongs to.</param>
        /// <returns>A task representing the asynchronous operation, containing a <see cref="NotificationDto"/> if the notification is found, or null if not.</returns>
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

        /// <summary>
        /// Creates a new notification for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user to create the notification for.</param>
        /// <param name="dto">The <see cref="NotificationCreateDto"/> containing the notification details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Marks a specific notification as read if it belongs to the specified user.
        /// </summary>
        /// <param name="id">The ID of the notification to mark as read.</param>
        /// <param name="userId">The ID of the user to verify the notification belongs to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MarkAsReadAsync(int id, int userId)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null || notification.UserId != userId) return;

            notification.MarkAsRead();
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a specific notification if it belongs to the specified user.
        /// </summary>
        /// <param name="id">The ID of the notification to delete.</param>
        /// <param name="userId">The ID of the user to verify the notification belongs to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteNotificationAsync(int id, int userId)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null || notification.UserId != userId) return;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes all notifications for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user to delete notifications for.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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
