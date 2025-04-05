using Moq;
using SolarflowServer.Models;
using SolarflowServer.Models.Enums;
using SolarflowServer.DTOs.Suggestion;
using SolarflowServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace SolarflowServer.Tests.Services
{
    public class SuggestionServiceTests
    {
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly ApplicationDbContext _context;
        private readonly SuggestionService _suggestionService;

        public SuggestionServiceTests()
        {
            // Configurar um banco de dados em memória para testes
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "SolarflowTestDb")
                .Options;

            _context = new ApplicationDbContext(options);

            // Mockar o serviço de notificação
            _mockNotificationService = new Mock<INotificationService>();

            // Inicializar o serviço de sugestões
            _suggestionService = new SuggestionService(_context, _mockNotificationService.Object);
        }

        [Fact]
        public async Task GenerateSuggestionsAsync_ShouldCreateSuggestions_WhenValidForecast()
        {
            // Arrange
            var battery = new Battery
            {
                ChargeLevel = 50,
                ChargingSource = "Grid", 
                BatteryMode = "Normal",
                MinimalTreshold = 20,
                MaximumTreshold = 100,
                SpendingStartTime = "00:00",
                SpendingEndTime = "06:00", 
                UserId = 1
            };
            battery.ID = 1;

            var forecast = new Forecast
            {
                BatteryID = 1,
                kwh = 4, // Simulando baixo valor de produção solar
                SolarHoursExpected = 3,
                WeatherCondition = "Partly Cloudy",
                ForecastDate = DateTime.UtcNow
            };

            await _context.Batteries.AddAsync(battery);
            await _context.Forecasts.AddAsync(forecast);
            await _context.SaveChangesAsync();

            // Act
            await _suggestionService.GenerateSuggestionsAsync(1);

            // Assert
            var suggestions = await _context.Suggestions.ToListAsync();
            Assert.Single(suggestions);
            Assert.Equal(SuggestionStatus.Pending, suggestions[0].Status);
            Assert.Equal(SuggestionType.ChargeAtNight, suggestions[0].Type);
        }

        [Fact]
        public async Task ApplySuggestionAsync_ShouldApplySuggestion_WhenValid()
        {
            // Arrange
            var battery = new Battery
            {
                ChargeLevel = 50,
                ChargingSource = "Grid", 
                BatteryMode = "Normal",
                MinimalTreshold = 20,
                MaximumTreshold = 100,
                SpendingStartTime = "00:00", 
                SpendingEndTime = "06:00",
                UserId = 1
            };

            var suggestion = new Suggestion
            {
                BatteryId = 1,
                Title = "Charge Battery at Night",
                Description = "Low solar forecast. Consider charging your battery using the grid during off-peak hours.",
                Status = SuggestionStatus.Pending,
                Type = SuggestionType.ChargeAtNight,
                TimeSent = DateTime.UtcNow
            };

            await _context.Batteries.AddAsync(battery);
            await _context.Suggestions.AddAsync(suggestion);
            await _context.SaveChangesAsync();

            // Act
            await _suggestionService.ApplySuggestionAsync(suggestion.Id);

            // Assert
            var updatedSuggestion = await _context.Suggestions.FindAsync(suggestion.Id);
            Assert.Equal(SuggestionStatus.Applied, updatedSuggestion.Status);
            Assert.Equal("00:00", battery.SpendingStartTime);
            Assert.Equal("06:00", battery.SpendingEndTime);
        }

        [Fact]
        public async Task IgnoreSuggestionAsync_ShouldIgnoreSuggestion_WhenValid()
        {
            // Arrange
            var battery = new Battery
            {
                ChargeLevel = 50,
                ChargingSource = "Grid", 
                BatteryMode = "Normal",
                MinimalTreshold = 20,
                MaximumTreshold = 100,
                SpendingStartTime = "00:00", 
                SpendingEndTime = "06:00", 
                UserId = 1
            };

            var suggestion = new Suggestion
            {
                BatteryId = 1,
                Title = "Enable Emergency Mode",
                Description = "Very low solar production forecast. Enable emergency mode to preserve energy.",
                Status = SuggestionStatus.Pending,
                Type = SuggestionType.EnableEmergencyMode,
                TimeSent = DateTime.UtcNow
            };

            await _context.Batteries.AddAsync(battery);
            await _context.Suggestions.AddAsync(suggestion);
            await _context.SaveChangesAsync();

            // Act
            await _suggestionService.IgnoreSuggestionAsync(suggestion.Id);

            // Assert
            var updatedSuggestion = await _context.Suggestions.FindAsync(suggestion.Id);
            Assert.Equal(SuggestionStatus.Ignored, updatedSuggestion.Status);
        }

        [Fact]
        public async Task CleanOldSuggestionsAsync_ShouldRemoveOldSuggestions()
        {
            // Arrange
            var oldSuggestion = new Suggestion
            {
                BatteryId = 1,
                Title = "Charge Battery at Night",
                Description = "Old suggestion for testing.",
                Status = SuggestionStatus.Pending,
                Type = SuggestionType.ChargeAtNight,
                TimeSent = DateTime.UtcNow.AddDays(-1) 
            };

            var recentSuggestion = new Suggestion
            {
                BatteryId = 1,
                Title = "Enable Emergency Mode",
                Description = "Current suggestion.",
                Status = SuggestionStatus.Pending,
                Type = SuggestionType.EnableEmergencyMode,
                TimeSent = DateTime.UtcNow
            };

            await _context.Suggestions.AddAsync(oldSuggestion);
            await _context.Suggestions.AddAsync(recentSuggestion);
            await _context.SaveChangesAsync();

            // Act
            await _suggestionService.CleanOldSuggestionsAsync();

            // Assert
            var suggestions = await _context.Suggestions.ToListAsync();
            Assert.Single(suggestions);
            Assert.Equal(recentSuggestion.Id, suggestions[0].Id);
        }
    }
}
