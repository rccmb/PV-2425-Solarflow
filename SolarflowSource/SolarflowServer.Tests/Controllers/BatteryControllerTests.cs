using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SolarflowServer.DTOs.SolarflowServer.DTOs;
using SolarflowServer.Models;
using SolarflowServer.Services.Interfaces;
using System.Security.Claims;

namespace SolarflowServer.Tests.Controllers

{
    public class BatteryControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly BatteryController _controller;

        public BatteryControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            _mockAuditService = new Mock<IAuditService>();
            _controller = new BatteryController(_context, _mockAuditService.Object);
        }

        [Fact]
        public async Task GetBattery_ReturnsOkResult_WhenBatteryExists()
        {
            var battery = new Battery
            {
                UserId = 1,
                ChargingSource = "Solar",
                BatteryMode = "Default",
                LastUpdate = DateTime.UtcNow.ToString(),
                SpendingStartTime = "08:00",
                SpendingEndTime = "18:00",
                MinimalTreshold = 20,
                MaximumTreshold = 80,
                ChargeLevel = 50
            };
            _context.Batteries.Add(battery);
            await _context.SaveChangesAsync();

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userClaims } };

            var result = await _controller.GetBattery();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UpdateBattery_ReturnsNotFound_WhenBatteryDoesNotExist()
        {
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userClaims } };

            var model = new BatteryDTO { ChargingSource = "Grid" };

            var result = await _controller.UpdateBattery(model);

            Assert.IsType<NotFoundObjectResult>(result);
        }
        [Fact]
        public async Task UpdateBattery_SuccessfulUpdate_ReturnsOk()
        {
            // Arrange: Criar uma bateria existente no banco de dados
            var battery = new Battery
            {
                UserId = 1,
                ChargingSource = "Solar",
                BatteryMode = "Default",
                LastUpdate = DateTime.UtcNow.ToString(),
                SpendingStartTime = "08:00",
                SpendingEndTime = "18:00",
                MinimalTreshold = 20,
                MaximumTreshold = 80,
                ChargeLevel = 50
            };

            _context.Batteries.Add(battery);
            await _context.SaveChangesAsync();

            // Simular o usuário autenticado com Id 1
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "1")
            }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userClaims } };

            // Criar o DTO com novos valores para a bateria
            var updateModel = new BatteryDTO
            {
                ChargingSource = "Grid",
                BatteryMode = "Eco",
                SpendingStartTime = "06:00",
                SpendingEndTime = "20:00",
                MinimalTreshold = 10,
                MaximumTreshold = 90
            };

            // Act: Chamar o método UpdateBattery
            var result = await _controller.UpdateBattery(updateModel);

            // Assert: Verificar se o retorno foi um OkObjectResult (HTTP 200)
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            // Verificar se a bateria foi atualizada corretamente no banco de dados
            var updatedBattery = await _context.Batteries.FirstOrDefaultAsync(b => b.UserId == 1);
            Assert.NotNull(updatedBattery);
            Assert.Equal("Grid", updatedBattery.ChargingSource);
            Assert.Equal("Eco", updatedBattery.BatteryMode);
            Assert.Equal("06:00", updatedBattery.SpendingStartTime);
            Assert.Equal("20:00", updatedBattery.SpendingEndTime);
            Assert.Equal(10, updatedBattery.MinimalTreshold);
            Assert.Equal(90, updatedBattery.MaximumTreshold);
        }


    }
}
