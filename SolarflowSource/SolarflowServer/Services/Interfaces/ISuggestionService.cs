using SolarflowServer.Models.Enums;
using SolarflowServer.DTOs.Suggestion;

// Defines the contract for handling energy usage suggestions
public interface ISuggestionService
{
    Task<List<SuggestionDto>> GetPendingSuggestionsAsync(int batteryId);
    Task ApplySuggestionAsync(int suggestionId);
    Task IgnoreSuggestionAsync(int suggestionId);
    Task GenerateSuggestionsAsync(int batteryId);
    Task CleanOldSuggestionsAsync();
}