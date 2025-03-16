using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SolarflowServer.Models;



namespace SolarflowServer.Controllers
{
    [Route("api/battery")]
    [ApiController]
    [Authorize]
    public class BatteryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BatteryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Create a new battery
        [HttpPost]
        public async Task<IActionResult> CreateBattery([FromBody] Battery battery)
        {
            if (battery == null)
                return BadRequest("Dados inválidos para a bateria.");

            _context.Batteries.Add(battery);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBatteryById), new { id = battery.ID }, battery);
        }


        // Get all batteries
        [HttpGet]
        public async Task<IActionResult> GetAllBatteries()
        {
            var batteries = await _context.Batteries.ToListAsync();
            return Ok(batteries);
        }

        // Get a single battery by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBatteryById(int id)
        {
            var battery = await _context.Batteries.FindAsync(id);
            if (battery == null)
                return NotFound("Battery not found.");

            return Ok(battery);
        }

        // Update a battery
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBattery(int id, [FromBody] Battery updatedBattery)
        {
            var battery = await _context.Batteries.FindAsync(id);
            if (battery == null)
                return NotFound("Battery not found.");

            battery.UserId = updatedBattery.UserId;
            battery.ApiKey = updatedBattery.ApiKey;
            battery.ChargeLevel = updatedBattery.ChargeLevel;
            battery.ChargingMode = updatedBattery.ChargingMode;
            battery.EmergencyMode = updatedBattery.EmergencyMode;
            battery.AutoOptimization = updatedBattery.AutoOptimization;
            battery.LastUpdate = updatedBattery.LastUpdate;

            await _context.SaveChangesAsync();
            return Ok(battery);
        }

        // Delete a battery
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBattery(int id)
        {
            var battery = await _context.Batteries.FindAsync(id);
            if (battery == null)
                return NotFound("Battery not found.");

            _context.Batteries.Remove(battery);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Battery deleted successfully." });
        }
    }
}
