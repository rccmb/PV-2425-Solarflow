using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SolarflowServer.DTOs.Notification;
using SolarflowServer.Models;
using SolarflowServer.Models.Enums;

namespace SolarflowServer.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;

        public NotificationService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int userId)
        {
            var notifications = await _repository.GetByUserIdAsync(userId);

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
            var notification = await _repository.GetByIdAsync(id);

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

            await _repository.AddAsync(notification);
            await _repository.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int id, int userId)
        {
            var notification = await _repository.GetByIdAsync(id);
            if (notification == null || notification.UserId != userId) return;

            notification.MarkAsRead();
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteNotificationAsync(int id, int userId)
        {
            var notification = await _repository.GetByIdAsync(id);
            if (notification == null || notification.UserId != userId) return;

            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteAllNotificationsAsync(int userId)
        {
            await _repository.DeleteAllAsync(userId);
            await _repository.SaveChangesAsync();
        }
    }
}
