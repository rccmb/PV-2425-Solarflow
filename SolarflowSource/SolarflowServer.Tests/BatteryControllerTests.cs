using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SolarflowServer.Models;
using SolarflowServer.Models.Enums;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Tests;

public class BatteryControllerTests
{
    private readonly ApplicationDbContext _context;
    private readonly BatteryController _controller;
    private readonly Mock<IAuditService> _mockAuditService;

    public BatteryControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase")
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
            ChargeSource = BatterySource.Solar,
            ChargeMode = BatteryMode.Normal,
            LastUpdate = DateTime.UtcNow,
            ChargeGridStartTime = new TimeSpan(0, 8, 0, 0),
            ChargeGridEndTime = new TimeSpan(0, 18, 0, 0),
            ThresholdMin = 20,
            ThresholdMax = 80,
            CapacityLevel = 50
        };
        _context.Batteries.Add(battery);
        await _context.SaveChangesAsync();

        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new(ClaimTypes.NameIdentifier, "1") }));
        _controller.ControllerContext = new ControllerContext
            { HttpContext = new DefaultHttpContext { User = userClaims } };

        var result = await _controller.GetBattery();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task UpdateBattery_SuccessfulUpdate_ReturnsOk()
    {
        // Arrange: Criar uma bateria existente no banco de dados
        var battery = new Battery
        {
            UserId = 1,
            ChargeSource = BatterySource.Solar,
            ChargeMode = BatteryMode.Normal,
            LastUpdate = DateTime.UtcNow,
            ChargeGridStartTime = new TimeSpan(0, 8, 0, 0),
            ChargeGridEndTime = new TimeSpan(0, 18, 0, 0),
            ThresholdMin = 20,
            ThresholdMax = 80,
            CapacityLevel = 50
        };

        _context.Batteries.Add(battery);
        await _context.SaveChangesAsync();

        // Simular o usuário autenticado com Id 1
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.NameIdentifier, "1")
        }));
        _controller.ControllerContext = new ControllerContext
            { HttpContext = new DefaultHttpContext { User = userClaims } };

        // Criar o DTO com novos valores para a bateria
        var updateModel = new BatteryDTO
        {
            ChargeMode = BatteryMode.Personalized,
            ChargeSource = BatterySource.Grid,
            ChargeGridStartTime = new TimeSpan(0, 6, 0, 0),
            ChargeGridEndTime = new TimeSpan(0, 20, 0, 0),
            ThresholdMin = 10,
            ThresholdMax = 90
        };

        // Act: Chamar o método UpdateBattery
        var result = await _controller.UpdateBattery(updateModel);

        // Assert: Verificar se o retorno foi um OkObjectResult (HTTP 200)
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Verificar se a bateria foi atualizada corretamente no banco de dados
        var updatedBattery = await _context.Batteries.FirstOrDefaultAsync(b => b.UserId == 1);
        Assert.NotNull(updatedBattery);
        Assert.Equal(BatterySource.Grid, updatedBattery.ChargeSource);
        Assert.Equal(BatteryMode.Personalized, updatedBattery.ChargeMode);
        Assert.Equal(new TimeSpan(0, 6, 0, 0), updatedBattery.ChargeGridStartTime);
        Assert.Equal(new TimeSpan(0, 20, 0, 0), updatedBattery.ChargeGridEndTime);
        Assert.Equal(10, updatedBattery.ThresholdMin);
        Assert.Equal(90, updatedBattery.ThresholdMax);
    }
}