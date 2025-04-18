﻿using Microsoft.EntityFrameworkCore;
using SolarflowServer.DTOs.Notification;
using SolarflowServer.DTOs.Suggestion;
using SolarflowServer.Models;
using SolarflowServer.Models.Enums;
using SolarflowServer.Services;

public class SuggestionService : ISuggestionService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    // Constructor injecting the database context and notification service
    public SuggestionService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    // Retrieves all pending suggestions for a given battery
    public async Task<List<SuggestionDto>> GetPendingSuggestionsAsync(int batteryId)
    {
        var battery = await _context.Batteries.FirstOrDefaultAsync(b => b.Id == batteryId);
        if (battery == null) return new List<SuggestionDto>();

        var suggestions = await _context.Suggestions
            .Where(s => s.BatteryId == batteryId && s.Status == SuggestionStatus.Pending)
            .OrderBy(s => s.TimeSent)
            .ToListAsync();

        // Maps each suggestion to a DTO for API use
        return suggestions.Select(s => new SuggestionDto
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            Status = s.Status.ToString(),
            Type = s.Type.ToString(),
            TimeSent = s.TimeSent
        }).ToList();
    }

    // Applies the logic behind a specific suggestion and updates the battery
    public async Task ApplySuggestionAsync(int suggestionId)
    {
        var suggestion = await _context.Suggestions
            .Include(s => s.Battery)
            .FirstOrDefaultAsync(s => s.Id == suggestionId);

        if (suggestion == null || suggestion.Status != SuggestionStatus.Pending)
            throw new InvalidOperationException("Suggestion not found or already handled.");

        // Executes the suggestion logic based on its type
        switch (suggestion.Type)
        {
            case SuggestionType.ChargeAtNight:
                suggestion.Battery.SpendingStartTime = "00:00";
                suggestion.Battery.SpendingEndTime = "06:00";
                break;

            case SuggestionType.EnableEmergencyMode:
                suggestion.Battery.BatteryMode = "Emergency";
                break;

            case SuggestionType.LowerBatteryThreshold:
                suggestion.Battery.MinimalTreshold = Math.Max(10, suggestion.Battery.MinimalTreshold - 10);
                break;

            case SuggestionType.RaiseBatteryThreshold:
                suggestion.Battery.MaximumTreshold = Math.Min(100, suggestion.Battery.MaximumTreshold + 10);
                break;

            default:
                throw new Exception("Unknown suggestion type.");
        }

        // Marks suggestion as applied and logs a notification for the user
        suggestion.Status = SuggestionStatus.Applied;
        await _notificationService.CreateNotificationAsync(
            suggestion.Battery.UserId,
            new NotificationCreateDto
            {
                Title = "Suggestion Applied",
                Description = $"Your suggestion '{suggestion.Title}' has been applied successfully."
            });

        await _context.SaveChangesAsync();
    }

    // Marks a suggestion as ignored
    public async Task IgnoreSuggestionAsync(int suggestionId)
    {
        var suggestion = await _context.Suggestions.FirstOrDefaultAsync(s => s.Id == suggestionId);
        if (suggestion == null || suggestion.Status != SuggestionStatus.Pending)
            throw new InvalidOperationException("Suggestion not found or already handled.");

        suggestion.Status = SuggestionStatus.Ignored;
        await _context.SaveChangesAsync();
    }

    // Generates suggestions based on the latest forecast and battery state
    public async Task GenerateSuggestionsAsync(int batteryId)
    {
        var battery = await _context.Batteries.FirstOrDefaultAsync(b => b.Id == batteryId);
        if (battery == null) return;

        var forecast = await _context.Forecasts
            .Where(f => f.BatteryID == battery.Id)
            .OrderByDescending(f => f.ForecastDate)
            .FirstOrDefaultAsync();

        if (forecast == null) return;

        var today = DateTime.UtcNow.Date;

        // Helper function to avoid duplicate suggestions of the same type for today
        async Task AddSuggestionIfNotExists(SuggestionType type, string title, string description)
        {
            var exists = await _context.Suggestions.AnyAsync(s =>
                s.BatteryId == battery.Id &&
                s.Type == type &&
                s.TimeSent.Date == today);

            if (!exists)
            {
                var suggestion = new Suggestion
                {
                    BatteryId = battery.Id,
                    Title = title,
                    Description = description,
                    Status = SuggestionStatus.Pending,
                    Type = type,
                    TimeSent = DateTime.UtcNow
                };
                _context.Suggestions.Add(suggestion);
            }
        }

        // Suggest charging at night if solar production is low
        if (forecast.kwh < 5)
        {
            await AddSuggestionIfNotExists(
                SuggestionType.ChargeAtNight,
                "Charge Battery at Night",
                "Low solar forecast. Consider charging your battery using the grid during off-peak hours."
            );
        }

        // Suggest enabling emergency mode if solar is very low
        if (forecast.kwh < 2 && battery.BatteryMode != "Emergency")
        {
            await AddSuggestionIfNotExists(
                SuggestionType.EnableEmergencyMode,
                "Enable Emergency Mode",
                "Very low solar production forecast. Enable emergency mode to preserve energy."
            );
        }

        // Suggest lowering the minimum threshold if battery is very full
        if (battery.ChargeLevel > 80 && battery.MinimalTreshold > 30)
        {
            await AddSuggestionIfNotExists(
                SuggestionType.LowerBatteryThreshold,
                "Lower Battery Threshold",
                "Battery is highly charged. You can reduce the minimum threshold for more flexibility."
            );
        }

        // Suggest raising the maximum threshold if battery is too low
        if (battery.ChargeLevel < 20 && battery.MaximumTreshold > 70)
        {
            await AddSuggestionIfNotExists(
                SuggestionType.RaiseBatteryThreshold,
                "Raise Battery Threshold",
                "Battery is low. Increase the maximum threshold to avoid over-discharge."
            );
        }

        await _context.SaveChangesAsync();
    }

    // Deletes all suggestions that were created before today
    public async Task CleanOldSuggestionsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var oldSuggestions = await _context.Suggestions
            .Where(s => s.TimeSent.Date < today)
            .ToListAsync();

        if (oldSuggestions.Any())
        {
            _context.Suggestions.RemoveRange(oldSuggestions);
            await _context.SaveChangesAsync();
        }
    }
}
