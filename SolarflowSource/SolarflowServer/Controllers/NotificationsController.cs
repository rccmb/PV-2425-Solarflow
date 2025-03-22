using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarflowServer.DTOs.Notification;
using SolarflowServer.Models;
using SolarflowServer.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SolarflowServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ApplicationDbContext _context;


        public NotificationsController(INotificationService notificationService, ApplicationDbContext context)
        {
            _notificationService = notificationService;
            _context = context;
        }

        // GET: api/notifications
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            var notifications = await _notificationService.GetNotificationsAsync(userId);
            return Ok(notifications);
        }

        // GET: api/notifications/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var userId = GetUserId();
            var notification = await _notificationService.GetNotificationByIdAsync(id, userId);
            if (notification == null) return NotFound();
            return Ok(notification);
        }

        // POST: api/notifications
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationCreateDto dto)
        {
            var userId = GetUserId();
            await _notificationService.CreateNotificationAsync(userId, dto);
            return CreatedAtAction(nameof(GetAll), null);
        }

        // PUT: api/notifications/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();
            await _notificationService.MarkAsReadAsync(id, userId);
            return NoContent();
        }

        // DELETE: api/notifications/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            await _notificationService.DeleteNotificationAsync(id, userId);
            return NoContent();
        }

        // DELETE: api/notifications
        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            var userId = GetUserId();
            await _notificationService.DeleteAllNotificationsAsync(userId);
            return NoContent();
        }
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


        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }


    }
}
