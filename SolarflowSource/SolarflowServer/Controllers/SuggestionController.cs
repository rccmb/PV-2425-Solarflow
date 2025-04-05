using Microsoft.AspNetCore.Mvc;
using SolarflowServer.Services;
using SolarflowServer.DTOs.Suggestion;
using System.Threading.Tasks;

namespace SolarflowServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuggestionController : ControllerBase
    {
        private readonly SuggestionService _suggestionService;

        public SuggestionController(SuggestionService suggestionService)
        {
            _suggestionService = suggestionService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSuggestions([FromQuery] int batteryId)
        {
            
            await _suggestionService.GenerateSuggestionsAsync(batteryId);

            return Ok(new { message = "Suggestions processed successfully" });
        }

        
        [HttpPost("apply/{id}")]
        public async Task<IActionResult> ApplySuggestion(int id)
        {
            await _suggestionService.ApplySuggestionAsync(id);
            return Ok(new { message = "Suggestion applied successfully." });
        }

        
        [HttpPost("ignore/{id}")]
        public async Task<IActionResult> IgnoreSuggestion(int id)
        {
            await _suggestionService.IgnoreSuggestionAsync(id);
            return Ok(new { message = "Suggestion ignored successfully." });
        }

      
        [HttpGet("get/{batteryId}")]
        public async Task<IActionResult> GetSuggestions(int batteryId)
        {
            var suggestions = await _suggestionService.GetPendingSuggestionsAsync(batteryId);

            if (suggestions == null || !suggestions.Any())
            {
                return NotFound(new { message = "No suggestions found for this battery." });
            }

            return Ok(suggestions);
        }
    }
}
