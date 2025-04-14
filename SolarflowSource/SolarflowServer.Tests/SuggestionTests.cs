using Microsoft.EntityFrameworkCore;
using Moq;
using SolarflowServer.Models;
using SolarflowServer.Models.Enums;
using SolarflowServer.Services;

namespace SolarflowServer.Tests.Services;

public class SuggestionServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly SuggestionService _suggestionService;

    public SuggestionServiceTests()
    {
        // Configure an in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("SolarflowTestDb")
            .Options;

        _context = new ApplicationDbContext(options);

        // Mock the notification service
        _mockNotificationService = new Mock<INotificationService>();

        // Initialize the SuggestionService
        _suggestionService = new SuggestionService(_context, _mockNotificationService.Object);

        // Create a battery to be reused in all tests
        var battery = new Battery
        {
            CapacityLevel = 50,
            ChargeSource = BatterySource.Grid,
            ChargeMode = BatteryMode.Normal,
            ThresholdMin = 20,
            ThresholdMax = 100,
            ChargeGridStartTime = new TimeSpan(0, 0, 0, 0),
            ChargeGridEndTime = new TimeSpan(0, 6, 0, 0),
            UserId = 1
        };
        battery.Id = 1;

        // Only add the battery if it doesn't already exist
        if (!_context.Batteries.Any(b => b.Id == battery.Id))
        {
            _context.Batteries.Add(battery);
            _context.SaveChanges();
        }
    }

    // Dispose method to clean up the database after each test
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GenerateSuggestionsAsync_ShouldCreateSuggestions_WhenValidForecast()
    {
        // Arrange
        var forecast = new Forecast
        {
            BatteryID = 1,
            kwh = 4, // Simulate low solar production
            SolarHoursExpected = 3,
            WeatherCondition = "Partly Cloudy",
            ForecastDate = DateTime.UtcNow
        };

        await _context.Forecasts.AddAsync(forecast);
        await _context.SaveChangesAsync();

        // Act
        await _suggestionService.GenerateSuggestionsAsync(1); // Pass user ID to the service

        // Assert
        var suggestions = await _context.Suggestions.ToListAsync();
        Assert.NotEmpty(suggestions);
        Assert.Equal(SuggestionStatus.Pending, suggestions[0].Status);
        Assert.Equal(SuggestionType.ChargeAtNight, suggestions[0].Type);
    }

    [Fact]
    public async Task ApplySuggestionAsync_ShouldApplySuggestion_WhenValid()
    {
        // Arrange
        var suggestion = new Suggestion
        {
            BatteryId = 1,
            Title = "Charge Battery at Night",
            Description = "Low solar forecast. Consider charging your battery using the grid during off-peak hours.",
            Status = SuggestionStatus.Pending,
            Type = SuggestionType.ChargeAtNight,
            TimeSent = DateTime.UtcNow
        };

        await _context.Suggestions.AddAsync(suggestion);
        await _context.SaveChangesAsync();

        // Act
        await _suggestionService.ApplySuggestionAsync(suggestion.Id);

        // Assert
        var updatedSuggestion = await _context.Suggestions.FindAsync(suggestion.Id);
        Assert.Equal(SuggestionStatus.Applied, updatedSuggestion.Status);
        Assert.Equal(new TimeSpan(0, 0, 0, 0), updatedSuggestion.Battery.ChargeGridStartTime);
        Assert.Equal(new TimeSpan(0, 9, 0, 0), updatedSuggestion.Battery.ChargeGridEndTime);
    }

    [Fact]
    public async Task IgnoreSuggestionAsync_ShouldIgnoreSuggestion_WhenValid()
    {
        // Arrange
        var suggestion = new Suggestion
        {
            BatteryId = 1,
            Title = "Enable Emergency Mode",
            Description = "Very low solar production forecast. Enable emergency mode to preserve energy.",
            Status = SuggestionStatus.Pending,
            Type = SuggestionType.EnableEmergencyMode,
            TimeSent = DateTime.UtcNow
        };

        await _context.Suggestions.AddAsync(suggestion);
        await _context.SaveChangesAsync();

        // Act
        await _suggestionService.IgnoreSuggestionAsync(suggestion.Id);

        // Assert
        var updatedSuggestion = await _context.Suggestions.FindAsync(suggestion.Id);
        Assert.Equal(SuggestionStatus.Ignored, updatedSuggestion.Status);
    }

    [Fact]
    public async Task GenerateSuggestionsAsync_ShouldNotCreateDuplicateSuggestion_WhenSuggestionExistsForToday()
    {
        // Arrange
        var forecast = new Forecast
        {
            BatteryID = 1,
            kwh = 4, // Simulating low solar production
            SolarHoursExpected = 3,
            WeatherCondition = "Partly Cloudy",
            ForecastDate = DateTime.UtcNow
        };

        await _context.Forecasts.AddAsync(forecast);
        await _context.SaveChangesAsync();

        // Adding an existing suggestion
        var existingSuggestion = new Suggestion
        {
            BatteryId = 1,
            Title = "Charge Battery at Night",
            Description = "Low solar forecast. Consider charging your battery using the grid during off-peak hours.",
            Status = SuggestionStatus.Pending,
            Type = SuggestionType.ChargeAtNight,
            TimeSent = DateTime.UtcNow
        };
        await _context.Suggestions.AddAsync(existingSuggestion);
        await _context.SaveChangesAsync();

        // Act: Generate suggestions again
        await _suggestionService.GenerateSuggestionsAsync(1); // Pass the same user ID again

        // Assert: Ensure no duplicate suggestion is created
        var suggestions = await _context.Suggestions.ToListAsync();
        var suggestionCount = suggestions.Count(s => s.Type == SuggestionType.ChargeAtNight && s.BatteryId == 1);
        Assert.Equal(1, suggestionCount); // There should be only one suggestion of this type
    }
}