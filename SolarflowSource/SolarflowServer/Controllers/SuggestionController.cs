using Microsoft.AspNetCore.Mvc;
using SolarflowServer.Services;
using SolarflowServer.DTOs.Suggestion;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SolarflowServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuggestionController : ControllerBase
    {
        private readonly SuggestionService _suggestionService;
        private readonly ApplicationDbContext _context;

        public SuggestionController(SuggestionService suggestionService, ApplicationDbContext context)
        {
            _suggestionService = suggestionService;
            _context = context;
        }

        // Create suggestions based on userId (now from the URL directly)
        [HttpPost("create")]
        public async Task<IActionResult> CreateSuggestions(int userId) // no [FromQuery] needed
        {
            // Find the battery associated with the user
            var battery = await _context.Batteries.FirstOrDefaultAsync(b => b.UserId == userId);

            if (battery == null)
            {
                return NotFound(new { message = "Battery not found for this user." });
            }

            // Generate suggestions for the battery
            await _suggestionService.GenerateSuggestionsAsync(battery.ID);

            return Ok(new { message = "Suggestions processed successfully" });
        }

        // Get pending suggestions based on userId
        [HttpGet("get/{userId}")]
        public async Task<IActionResult> GetSuggestions(int userId)
        {
            // Find the battery associated with the user
            var battery = await _context.Batteries.FirstOrDefaultAsync(b => b.UserId == userId);

            if (battery == null)
            {
                return NotFound(new { message = "Battery not found for this user." });
            }

            // Get pending suggestions for the battery
            var suggestions = await _suggestionService.GetPendingSuggestionsAsync(battery.ID);

            if (suggestions == null || !suggestions.Any())
            {
                return NotFound(new { message = "No suggestions found for this battery." });
            }

            return Ok(suggestions);
        }

        // Apply a suggestion
        [HttpPost("apply/{id}")]
        public async Task<IActionResult> ApplySuggestion(int id)
        {
            await _suggestionService.ApplySuggestionAsync(id);
            return Ok(new { message = "Suggestion applied successfully." });
        }

        // Ignore a suggestion
        [HttpPost("ignore/{id}")]
        public async Task<IActionResult> IgnoreSuggestion(int id)
        {
            await _suggestionService.IgnoreSuggestionAsync(id);
            return Ok(new { message = "Suggestion ignored successfully." });
        }

        // Clean old suggestions (pending and ignored)
        [HttpPost("clean")]
        public async Task<IActionResult> CleanOldSuggestions()
        {
            await _suggestionService.CleanOldSuggestionsAsync();
            return Ok(new { message = "Old suggestions cleaned successfully." });
        }
    }
}
