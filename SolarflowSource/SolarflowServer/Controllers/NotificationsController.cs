using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarflowServer.DTOs.Notification;
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

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
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

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
