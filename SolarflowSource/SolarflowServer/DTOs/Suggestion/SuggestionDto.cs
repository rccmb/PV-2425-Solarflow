using SolarflowServer.Models.Enums;

namespace SolarflowServer.DTOs.Suggestion
{
    /// <summary>
    /// Represents the data transfer object for a suggestion.
    /// </summary>
    public class SuggestionDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the suggestion.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the suggestion.
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// Gets or sets the description or details of the suggestion.
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Gets or sets the current status of the suggestion.
        /// </summary>
        /// <remarks>
        /// The status could represent states such as "Pending", "Approved", or "Rejected".
        /// </remarks>
        public string Status { get; set; } = "";

        /// <summary>
        /// Gets or sets the type or category of the suggestion.
        /// </summary>
        /// <remarks>
        /// The type could represent categories such as "Feature Request" or "Bug Report".
        /// </remarks>
        public string Type { get; set; } = "";

        /// <summary>
        /// Gets or sets the date and time when the suggestion was submitted.
        /// </summary>
        public DateTime TimeSent { get; set; }
    }

}