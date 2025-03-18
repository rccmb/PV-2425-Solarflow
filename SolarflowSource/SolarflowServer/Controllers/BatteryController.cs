using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SolarflowServer.Models;
using System.Security.Claims;
using System.Net.Http;
using Microsoft.AspNetCore.Identity;
using SolarflowServer.DTOs.SolarflowServer.DTOs; // Make sure to import the DTO



namespace SolarflowServer.Controllers
{
    [Route("api/battery")]
    [ApiController]
    [Authorize]
    public class BatteryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BatteryController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Create a new battery
        [HttpPost("create-battery")]
        public async Task<IActionResult> CreateBattery([FromBody] Battery battery)
        {
            if (battery == null)
                return BadRequest("Dados inválidos para a bateria.");

            _context.Batteries.Add(battery);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBatteryById), new { id = battery.ID }, battery);
        }


        // Get all batteries
        [HttpGet("get-all-batteries")]
        public async Task<IActionResult> GetAllBatteries()
        {
            var batteries = await _context.Batteries.ToListAsync();
            return Ok(batteries);
        }

        // Get a single battery by ID
        [HttpGet("get-one-battery")]
        public async Task<IActionResult> GetBatteryById([FromQuery] int id)
        {
            var battery = await _context.Batteries.FindAsync(id);
            if (battery == null)
                return NotFound("Battery not found.");

            return Ok(battery);
        }

        // Update a battery
        [HttpPut("update-battery")]
        public async Task<IActionResult> UpdateBattery(int id, [FromBody] Battery updatedBattery)
        {
            var battery = await _context.Batteries.FindAsync(id);
            if (battery == null)
                return NotFound("Battery not found.");

            battery.UserId = updatedBattery.UserId;
            battery.ApiKey = updatedBattery.ApiKey;
            battery.ChargeLevel = updatedBattery.ChargeLevel;
            battery.ChargingSource = updatedBattery.ChargingSource;
            battery.BatteryMode = updatedBattery.BatteryMode;
            battery.MinimalTreshold = updatedBattery.MinimalTreshold;
            battery.MaximumTreshold = updatedBattery.MaximumTreshold;
            battery.SpendingStartTime = updatedBattery.SpendingStartTime;
            battery.SpendingEndTime = updatedBattery.SpendingEndTime;
            battery.LastUpdate = updatedBattery.LastUpdate;

            await _context.SaveChangesAsync();
            return Ok(battery);
        }

        // Delete a battery
        [HttpDelete("delete-battery")]
        public async Task<IActionResult> DeleteBattery(int id)
        {
            var battery = await _context.Batteries.FindAsync(id);
            if (battery == null)
                return NotFound("Battery not found.");

            _context.Batteries.Remove(battery);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Battery deleted successfully." });
        }

        [HttpPost("get-user-battery")]
        public async Task<IActionResult> GetUserBattery()
        {
            var userEmail = Request.Cookies["UserEmail"];  // Fetch UserEmail from the cookie
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User email not found.");
            }

            var user = await _userManager.FindByEmailAsync(userEmail); // Find user by email
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var battery = await _context.Batteries.FirstOrDefaultAsync(b => b.UserId == user.Id); // Fetch battery by UserId
            if (battery == null)
            {
                return NotFound("Battery not found.");
            }

            // Map Battery model to GetBatteryDto
            var batteryDto = new GetBatteryDto
            {
                ID = battery.ID,
                UserId = battery.UserId,
                ApiKey = battery.ApiKey,
                ChargeLevel = battery.ChargeLevel,
                ChargingSource = battery.ChargingSource,
                BatteryMode = battery.BatteryMode,
                MinimalTreshold = battery.MinimalTreshold,
                MaximumTreshold = battery.MaximumTreshold,
                SpendingStartTime = battery.SpendingStartTime,
                SpendingEndTime = battery.SpendingEndTime,
                LastUpdate = battery.LastUpdate
            };

            return Ok(batteryDto); // Return the battery info as a DTO
        }


    }
}
