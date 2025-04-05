using Moq;
using System.Threading.Tasks;
using Xunit;
using SolarflowServer.Services;
using SolarflowServer.DTOs.Notification;
using SolarflowServer.Models;
using System.Collections.Generic;
using SolarflowServer.Models.Enums;

namespace SolarflowServer.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _repositoryMock;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _repositoryMock = new Mock<INotificationRepository>();
            _service = new NotificationService(_repositoryMock.Object);
        }

        [Fact]
        public async Task CreateNotificationAsync_Should_Call_Add_And_Save()
        {
            // Arrange
            int userId = 45;
            var dto = new NotificationCreateDto
            {
                Title = "Test",
                Description = "This is a test notification"
            };

            // Act
            await _service.CreateNotificationAsync(userId, dto);

            // Assert
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetNotificationsAsync_Should_Return_Notifications_For_User()
        {
            // Arrange
            int userId = 99;
            _repositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(new List<Notification>
            {
                new Notification { Id = 1, Title = "Test", Description = "Desc", UserId = userId, Status = NotificationStatus.Unread }
            });

            // Act
            var result = await _service.GetNotificationsAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Test", result.First().Title);
        }

        [Fact]
        public async Task MarkAsReadAsync_Should_Update_Status_If_Notification_Owned_By_User()
        {
            // Arrange
            int userId = 99;
            var notification = new Notification
            {
                Id = 1,
                Title = "Test",
                UserId = userId,
                Status = NotificationStatus.Unread
            };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(notification);

            // Act
            await _service.MarkAsReadAsync(1, userId);

            // Assert
            Assert.Equal(NotificationStatus.Read, notification.Status);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteNotificationAsync_Should_Remove_Notification_If_Owned_By_User()
        {
            // Arrange
            int userId = 99;
            var notification = new Notification { Id = 1, UserId = userId };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(notification);

            // Act
            await _service.DeleteNotificationAsync(1, userId);

            // Assert
            _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAllNotificationsAsync_Should_Call_DeleteAll_And_Save()
        {
            // Arrange
            int userId = 99;

            // Act
            await _service.DeleteAllNotificationsAsync(userId);

            // Assert
            _repositoryMock.Verify(r => r.DeleteAllAsync(userId), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
