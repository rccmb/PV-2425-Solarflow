using Microsoft.EntityFrameworkCore;
using SolarflowServer.DTOs.Notification;
using SolarflowServer.Models;
using SolarflowServer.Models.Enums;
using SolarflowServer.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SolarflowServer.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "NotificationTestDb_" + System.Guid.NewGuid())
                .EnableSensitiveDataLogging() // Optional: helps with debugging
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new NotificationService(_context);
        }

        [Fact]
        public async Task CreateNotificationAsync_Should_Add_Notification()
        {
            int userId = 45;
            var dto = new NotificationCreateDto
            {
                Title = "Test",
                Description = "This is a test notification"
            };

            await _service.CreateNotificationAsync(userId, dto);

            var saved = await _context.Notifications.FirstOrDefaultAsync(n => n.UserId == userId);
            Assert.NotNull(saved);
            Assert.Equal("Test", saved.Title);
        }

        [Fact]
        public async Task GetNotificationsAsync_Should_Return_Only_Users_Notifications()
        {
            _context.Notifications.AddRange(
                new Notification
                {
                    UserId = 1,
                    Title = "User 1",
                    Description = "Desc 1",
                    Status = NotificationStatus.Unread,
                    TimeSent = DateTime.UtcNow
                },
                new Notification
                {
                    UserId = 2,
                    Title = "User 2",
                    Description = "Desc 2",
                    Status = NotificationStatus.Unread,
                    TimeSent = DateTime.UtcNow
                }
            );
            await _context.SaveChangesAsync();

            var result = await _service.GetNotificationsAsync(1);

            Assert.Single(result);
            Assert.Equal("User 1", result.First().Title);
        }

        [Fact]
        public async Task MarkAsReadAsync_Should_Change_Status()
        {
            var notification = new Notification
            {
                UserId = 99,
                Title = "Unread Notification",
                Description = "Something unread",
                Status = NotificationStatus.Unread,
                TimeSent = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _service.MarkAsReadAsync(notification.Id, 99);

            var updated = await _context.Notifications.FindAsync(notification.Id);
            Assert.Equal(NotificationStatus.Read, updated.Status);
        }

        [Fact]
        public async Task DeleteNotificationAsync_Should_Remove_Notification()
        {
            var notification = new Notification
            {
                UserId = 50,
                Title = "Delete Me",
                Description = "Desc to delete",
                TimeSent = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _service.DeleteNotificationAsync(notification.Id, 50);

            var exists = await _context.Notifications.FindAsync(notification.Id);
            Assert.Null(exists);
        }

        [Fact]
        public async Task DeleteAllNotificationsAsync_Should_Clear_Users_Notifications()
        {
            _context.Notifications.AddRange(
                new Notification { UserId = 77, Title = "A", Description = "D1", TimeSent = DateTime.UtcNow },
                new Notification { UserId = 77, Title = "B", Description = "D2", TimeSent = DateTime.UtcNow },
                new Notification { UserId = 88, Title = "C", Description = "D3", TimeSent = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            await _service.DeleteAllNotificationsAsync(77);

            var remaining = await _context.Notifications.Where(n => n.UserId == 77).ToListAsync();
            Assert.Empty(remaining);
        }
    }
}
