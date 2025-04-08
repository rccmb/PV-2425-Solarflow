using SolarflowServer.Models.Enums;

namespace SolarflowServer.DTOs.Suggestion
{

    public class SuggestionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public string Type { get; set; } = "";
        public DateTime TimeSent { get; set; }
    }

}