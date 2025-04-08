namespace SolarflowClient.Models.ViewModels.Suggestions
{
    public class SuggestionViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class GetSuggestionsViewModel
    {
        public List<SuggestionViewModel> Suggestions { get; set; } = new();
    }
}