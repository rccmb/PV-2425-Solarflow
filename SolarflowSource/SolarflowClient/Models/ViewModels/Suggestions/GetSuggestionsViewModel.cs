namespace SolarflowClient.Models.ViewModels.Suggestions
{
    public class GetSuggestionsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } 
        public string Type { get; set; }    
        public DateTime TimeSent { get; set; }
    }
}