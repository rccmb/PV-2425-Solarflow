using System.Collections.Generic;
using System.Threading.Tasks;
using SolarflowServer.DTOs.Notification;

namespace SolarflowServer.Services
{
    /// <summary>
    /// Defines the contract for managing notifications, including creation, retrieval, updates, and deletion.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Retrieves all notifications for a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose notifications are being retrieved.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a collection of <see cref="NotificationDto"/> objects.
        /// </returns>
        Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int userId);

        /// <summary>
        /// Retrieves a specific notification by its ID for a specific user.
        /// </summary>
        /// <param name="id">The unique identifier of the notification.</param>
        /// <param name="userId">The unique identifier of the user who owns the notification.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the <see cref="NotificationDto"/> object for the specified notification.
        /// </returns>
        Task<NotificationDto> GetNotificationByIdAsync(int id, int userId);

        /// <summary>
        /// Creates a new notification for a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user for whom the notification is being created.</param>
        /// <param name="dto">The data transfer object containing the details of the notification to be created.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task CreateNotificationAsync(int userId, NotificationCreateDto dto);

        /// <summary>
        /// Marks a specific notification as read for a specific user.
        /// </summary>
        /// <param name="id">The unique identifier of the notification to be marked as read.</param>
        /// <param name="userId">The unique identifier of the user who owns the notification.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task MarkAsReadAsync(int id, int userId);

        /// <summary>
        /// Deletes a specific notification for a specific user.
        /// </summary>
        /// <param name="id">The unique identifier of the notification to be deleted.</param>
        /// <param name="userId">The unique identifier of the user who owns the notification.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteNotificationAsync(int id, int userId);

        /// <summary>
        /// Deletes all notifications for a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose notifications are being deleted.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteAllNotificationsAsync(int userId);
    }
}