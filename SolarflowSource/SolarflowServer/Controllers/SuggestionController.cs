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
    /// <summary>
    /// Controller for managing suggestions related to the battery of the authenticated user.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SuggestionController : ControllerBase
    {
        private readonly ISuggestionService _suggestionService;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SuggestionController"/> class.
        /// </summary>
        /// <param name="suggestionService">The service for managing suggestions.</param>
        /// <param name="context">The database context.</param>
        public SuggestionController(ISuggestionService suggestionService, ApplicationDbContext context)
        {
            _suggestionService = suggestionService;
            _context = context;
        }

        /// <summary>
        /// Generates suggestions for the battery associated with the given user.
        /// </summary>
        /// <param name="userId">The user ID for whom the suggestions are being generated.</param>
        /// <returns>A response indicating success or failure.</returns>
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

        /// <summary>
        /// Returns all pending suggestions for the user's battery.
        /// </summary>
        /// <param name="userId">The user ID for whom the suggestions are being fetched.</param>
        /// <returns>A list of pending suggestions or a not found response.</returns>
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

        /// <summary>
        /// Applies the logic of a suggestion and updates the battery accordingly.
        /// </summary>
        /// <param name="id">The ID of the suggestion to apply.</param>
        /// <returns>A response indicating the success of the operation.</returns>
        [HttpPost("apply/{id}")]
        public async Task<IActionResult> ApplySuggestion(int id)
        {
            await _suggestionService.ApplySuggestionAsync(id);
            return Ok(new { message = "Suggestion applied successfully." });
        }

        /// <summary>
        /// Marks a suggestion as ignored, preventing it from being applied.
        /// </summary>
        /// <param name="id">The ID of the suggestion to ignore.</param>
        /// <returns>A response indicating the success of the operation.</returns>
        [HttpPost("ignore/{id}")]
        public async Task<IActionResult> IgnoreSuggestion(int id)
        {
            await _suggestionService.IgnoreSuggestionAsync(id);
            return Ok(new { message = "Suggestion ignored successfully." });
        }

        /// <summary>
        /// Removes all suggestions older than the current date.
        /// </summary>
        /// <returns>A response indicating the success of the operation.</returns>
        [HttpPost("clean")]
        public async Task<IActionResult> CleanOldSuggestions()
        {
            await _suggestionService.CleanOldSuggestionsAsync();
            return Ok(new { message = "Old suggestions cleaned successfully." });
        }

    }
}
