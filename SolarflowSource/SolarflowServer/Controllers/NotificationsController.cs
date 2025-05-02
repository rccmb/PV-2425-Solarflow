using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarflowServer.DTOs.Notification;
using SolarflowServer.Models;
using SolarflowServer.Services;
using SolarflowServer.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using SolarflowServer.Models.Enums;

namespace SolarflowServer.Controllers
{
    /// <summary>
    /// Controller for managing notifications for the authenticated user.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ensures only authenticated users can access this controller
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsController"/> class.
        /// </summary>
        /// <param name="notificationService">The service for managing notifications.</param>
        /// <param name="context">The database context.</param>
        /// <param name="auditService">The service for auditing actions.</param>
        public NotificationsController(INotificationService notificationService, ApplicationDbContext context, IAuditService auditService)
        {
            _notificationService = notificationService;
            _context = context;
            _auditService = auditService;
        }

        /// <summary>
        /// Retrieves all notifications for the authenticated user.
        /// </summary>
        /// <returns>A list of notifications.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            var notifications = await _notificationService.GetNotificationsAsync(userId);

            // Optional audit logging
            return Ok(notifications);
        }

        /// <summary>
        /// Retrieves a specific notification by ID if it belongs to the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the notification to retrieve.</param>
        /// <returns>A specific notification if found, or a not found response.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var userId = GetUserId();
            var notification = await _notificationService.GetNotificationByIdAsync(id, userId);
            if (notification == null) return NotFound();

            return Ok(notification);
        }

        /// <summary>
        /// Creates a new notification for the authenticated user.
        /// </summary>
        /// <param name="dto">The notification data to create.</param>
        /// <returns>A created notification response.</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationCreateDto dto)
        {
            var userId = GetUserId();
            await _notificationService.CreateNotificationAsync(userId, dto);

            return CreatedAtAction(nameof(GetAll), null);
        }

        /// <summary>
        /// Marks a specific notification as read if it belongs to the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the notification to mark as read.</param>
        /// <returns>A no content response if successful.</returns>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();
            await _notificationService.MarkAsReadAsync(id, userId);

            return NoContent();
        }

        /// <summary>
        /// Deletes a specific notification by ID if it belongs to the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the notification to delete.</param>
        /// <returns>A no content response if successful.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            await _notificationService.DeleteNotificationAsync(id, userId);

            return NoContent();
        }

        /// <summary>
        /// Deletes all notifications for the authenticated user.
        /// </summary>
        /// <returns>A no content response if successful.</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            var userId = GetUserId();
            await _notificationService.DeleteAllNotificationsAsync(userId);

            return NoContent();
        }

        /// <summary>
        /// Helper method to retrieve the authenticated user's ID from the JWT.
        /// </summary>
        /// <returns>The user ID of the authenticated user.</returns>
        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // Retrieves the client IP address (useful for auditing)
        private string GetClientIPAddress()
        {
            if (HttpContext?.Connection?.RemoteIpAddress == null)
            {
                return "127.0.0.1"; // Default for testing
            }

            return HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }
}
