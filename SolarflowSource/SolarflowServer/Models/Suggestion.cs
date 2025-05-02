using SolarflowServer.Models;
using SolarflowServer.Models.Enums;

namespace SolarflowServer.Models
{
    /// <summary>
    /// Represents a suggestion for optimizing battery usage or settings.
    /// </summary>
    public class Suggestion
    {
        /// <summary>
        /// Gets or sets the unique identifier for the suggestion.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the suggestion.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description or details of the suggestion.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the suggestion (e.g., ChargeAtNight, EnableEmergencyMode).
        /// </summary>
        public SuggestionType Type { get; set; }

        /// <summary>
        /// Gets or sets the current status of the suggestion (e.g., Pending, Applied, Ignored).
        /// </summary>
        public SuggestionStatus Status { get; set; } = SuggestionStatus.Pending;

        /// <summary>
        /// Gets or sets the timestamp of when the suggestion was created.
        /// </summary>
        public DateTime TimeSent { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the unique identifier of the battery associated with the suggestion.
        /// </summary>
        public int BatteryId { get; set; }

        /// <summary>
        /// Gets or sets the battery associated with the suggestion.
        /// </summary>
        public Battery Battery { get; set; } = null!;
    }
}