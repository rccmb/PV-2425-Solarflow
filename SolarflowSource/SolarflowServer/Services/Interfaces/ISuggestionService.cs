using SolarflowServer.Models.Enums;
using SolarflowServer.DTOs.Suggestion;

/// <summary>
/// Defines the contract for handling energy usage suggestions, including retrieval, application, and cleanup operations.
/// </summary>
public interface ISuggestionService
{
    /// <summary>
    /// Retrieves all pending suggestions for a specific battery.
    /// </summary>
    /// <param name="batteryId">The unique identifier of the battery for which suggestions are being retrieved.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of <see cref="SuggestionDto"/> objects representing the pending suggestions.
    /// </returns>
    Task<List<SuggestionDto>> GetPendingSuggestionsAsync(int batteryId);

    /// <summary>
    /// Applies a specific suggestion by its unique identifier.
    /// </summary>
    /// <param name="suggestionId">The unique identifier of the suggestion to be applied.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ApplySuggestionAsync(int suggestionId);

    /// <summary>
    /// Ignores a specific suggestion by its unique identifier.
    /// </summary>
    /// <param name="suggestionId">The unique identifier of the suggestion to be ignored.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task IgnoreSuggestionAsync(int suggestionId);

    /// <summary>
    /// Generates new suggestions for a specific battery based on its current state and usage patterns.
    /// </summary>
    /// <param name="batteryId">The unique identifier of the battery for which suggestions are being generated.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task GenerateSuggestionsAsync(int batteryId);

    /// <summary>
    /// Cleans up old suggestions that are no longer relevant or have expired.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CleanOldSuggestionsAsync();
}