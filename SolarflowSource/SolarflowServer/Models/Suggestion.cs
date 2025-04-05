using SolarflowServer.Models;
using SolarflowServer.Models.Enums;

namespace SolarflowServer.Models
{
    public class Suggestion
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public SuggestionType Type { get; set; }
        public SuggestionStatus Status { get; set; } = SuggestionStatus.Pending;

        public DateTime TimeSent { get; set; } = DateTime.UtcNow;

        public int BatteryId { get; set; }
        public Battery Battery { get; set; } = null!;
    }
}