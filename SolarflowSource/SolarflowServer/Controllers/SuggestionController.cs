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

        [HttpPost("clean")]
        public async Task<IActionResult> CleanOldSuggestions()
        {
            await _suggestionService.CleanOldSuggestionsAsync();
            return Ok(new { message = "Old suggestions cleaned successfully." });
        }

        [HttpPost("add-test-suggestions")]
        public async Task<IActionResult> AddTestSuggestions()
        {
            var battery = await _context.Batteries.FirstOrDefaultAsync(b => b.UserId == 1);
            if (battery == null)
            {
                return NotFound(new { message = "Battery not found for this user." });
            }

            // Adiciona a primeira sugestão de teste
            var suggestion1 = new Suggestion
            {
                BatteryId = 1,
                Title = "Test Suggestion 1",
                Description = "This is the first test suggestion.",
                Status = SuggestionStatus.Pending,
                Type = SuggestionType.ChargeAtNight,
                TimeSent = DateTime.UtcNow
            };

            // Adiciona a segunda sugestão de teste
            var suggestion2 = new Suggestion
            {
                BatteryId = 1,
                Title = "Test Suggestion 2",
                Description = "This is the second test suggestion.",
                Status = SuggestionStatus.Pending,
                Type = SuggestionType.EnableEmergencyMode,
                TimeSent = DateTime.UtcNow
            };

            _context.Suggestions.Add(suggestion1);
            _context.Suggestions.Add(suggestion2);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Test suggestions added successfully." });
        }
    }
}
