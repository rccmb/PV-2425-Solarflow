namespace SolarflowClient.Models.ViewModels.Suggestions
{
    /// <summary>
    /// Represents a single suggestion with its details.
    /// </summary>
    public class SuggestionViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the suggestion.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the suggestion.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description or details of the suggestion.
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Represents the data required to display a collection of suggestions.
    /// </summary>
    public class GetSuggestionsViewModel
    {
        /// <summary>
        /// Gets or sets the collection of suggestions.
        /// </summary>
        public List<SuggestionViewModel> Suggestions { get; set; } = new();
    }
}