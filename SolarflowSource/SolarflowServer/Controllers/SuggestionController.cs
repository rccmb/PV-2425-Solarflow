using Microsoft.AspNetCore.Mvc;
using SolarflowServer.Services;
using SolarflowServer.DTOs.Suggestion;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SolarflowServer.Models.Enums;
using SolarflowServer.Models;

namespace SolarflowServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuggestionController : ControllerBase
    {
        private readonly ISuggestionService _suggestionService;
        private readonly ApplicationDbContext _context;

        public SuggestionController(ISuggestionService suggestionService, ApplicationDbContext context)
        {
            _suggestionService = suggestionService;
            _context = context;
        }

        // POST: api/suggestion/create/{userId}
        // Generates suggestions for the battery associated with the given user
        [HttpPost("create/{userId}")]
        public async Task<IActionResult> CreateSuggestions(int userId)
        {
            var battery = await _context.Batteries.FirstOrDefaultAsync(b => b.UserId == userId);
            if (battery == null)
            {
                return NotFound(new { message = "Battery not found for this user." });
            }

            await _suggestionService.GenerateSuggestionsAsync(battery.Id);
            return Ok(new { message = "Suggestions processed successfully" });
        }

        // GET: api/suggestion/get/{userId}
        // Returns all pending suggestions for the user's battery
        [HttpGet("get/{userId}")]
        public async Task<IActionResult> GetSuggestions(int userId)
        {
            var suggestions = await _suggestionService.GetPendingSuggestionsAsync(userId);
            if (!suggestions.Any())
            {
                return NotFound(new { message = "No suggestions found for this battery." });
            }

            return Ok(suggestions);
        }

        // POST: api/suggestion/apply/{id}
        // Applies the logic of a suggestion and updates the battery
        [HttpPost("apply/{id}")]
        public async Task<IActionResult> ApplySuggestion(int id)
        {
            await _suggestionService.ApplySuggestionAsync(id);
            return Ok(new { message = "Suggestion applied successfully." });
        }

        // POST: api/suggestion/ignore/{id}
        // Marks a suggestion as ignored (won't be applied)
        [HttpPost("ignore/{id}")]
        public async Task<IActionResult> IgnoreSuggestion(int id)
        {
            await _suggestionService.IgnoreSuggestionAsync(id);
            return Ok(new { message = "Suggestion ignored successfully." });
        }

        // POST: api/suggestion/clean
        // Removes all suggestions older than today
        [HttpPost("clean")]
        public async Task<IActionResult> CleanOldSuggestions()
        {
            await _suggestionService.CleanOldSuggestionsAsync();
            return Ok(new { message = "Old suggestions cleaned successfully." });
        }

        // POST: api/suggestion/add-test-suggestions
        // Creates two sample suggestions for testing/demo purposes
        [HttpPost("add-test-suggestions")]
        public async Task<IActionResult> AddTestSuggestions()
        {
            var battery = await _context.Batteries.FirstOrDefaultAsync(b => b.UserId == 1);
            if (battery == null)
            {
                return NotFound(new { message = "Battery not found for this user." });
            }

            var suggestion1 = new Suggestion
            {
                BatteryId = 1,
                Title = "Test Suggestion 1",
                Description = "This is the first test suggestion.",
                Status = SuggestionStatus.Pending,
                Type = SuggestionType.ChargeAtNight,
                TimeSent = DateTime.UtcNow
            };

            var suggestion2 = new Suggestion
            {
                BatteryId = 1,
                Title = "Test Suggestion 2",
                Description = "This is the second test suggestion.",
                Status = SuggestionStatus.Pending,
                Type = SuggestionType.RaiseBatteryThreshold,
                TimeSent = DateTime.UtcNow
            };

            _context.Suggestions.Add(suggestion1);
            _context.Suggestions.Add(suggestion2);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Test suggestions added successfully." });
        }
    }
}
