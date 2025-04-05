using SolarflowServer.Models.Enums;
using SolarflowServer.DTOs.Suggestion;

public interface ISuggestionService
{
    Task<List<SuggestionDto>> GetPendingSuggestionsAsync(int batteryId);
    Task ApplySuggestionAsync(int suggestionId);
    Task IgnoreSuggestionAsync(int suggestionId);
    Task GenerateSuggestionsAsync(int batteryId);
    Task CleanOldSuggestionsAsync();
}