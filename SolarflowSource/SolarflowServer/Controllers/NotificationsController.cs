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
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ensures only authenticated users can access this controller
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public NotificationsController(INotificationService notificationService, ApplicationDbContext context, IAuditService auditService)
        {
            _notificationService = notificationService;
            _context = context;
            _auditService = auditService;
        }

        // GET: api/notifications
        // Retrieves all notifications for the authenticated user
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            var notifications = await _notificationService.GetNotificationsAsync(userId);

            // Optional audit logging
            return Ok(notifications);
        }

        // GET: api/notifications/{id}
        // Retrieves a specific notification if it belongs to the user
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var userId = GetUserId();
            var notification = await _notificationService.GetNotificationByIdAsync(id, userId);
            if (notification == null) return NotFound();

            return Ok(notification);
        }

        // POST: api/notifications
        // Creates a new notification for the authenticated user
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationCreateDto dto)
        {
            var userId = GetUserId();
            await _notificationService.CreateNotificationAsync(userId, dto);

            return CreatedAtAction(nameof(GetAll), null);
        }

        // PUT: api/notifications/{id}/read
        // Marks a notification as read if it belongs to the user
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();
            await _notificationService.MarkAsReadAsync(id, userId);

            return NoContent();
        }

        // DELETE: api/notifications/{id}
        // Deletes a specific notification if it belongs to the user
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            await _notificationService.DeleteNotificationAsync(id, userId);

            return NoContent();
        }

        // DELETE: api/notifications
        // Deletes all notifications for the user
        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            var userId = GetUserId();
            await _notificationService.DeleteAllNotificationsAsync(userId);

            return NoContent();
        }

        // POST: api/notifications/generate-test
        // Generates dummy notifications for testing purposes
        [HttpPost("generate-test")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateTestNotifications()
        {
            var testNotifications = new List<Notification>
            {
                new Notification { Title = "New Feature!", Description = "Check out the new notification module.", TimeSent = DateTime.UtcNow, UserId = 1 },
                new Notification { Title = "Reminder", Description = "Battery needs calibration.", TimeSent = DateTime.UtcNow.AddHours(-1), UserId = 1 },
                new Notification { Title = "Success", Description = "Your profile was updated.", TimeSent = DateTime.UtcNow.AddDays(-1), UserId = 1, Status = NotificationStatus.Read, TimeRead = DateTime.UtcNow.AddDays(-1).AddMinutes(5) }
            };

            _context.Notifications.AddRange(testNotifications);
            await _context.SaveChangesAsync();

            return Ok("Test notifications created.");
        }

        // Helper method to get the authenticated user's ID from the JWT
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
