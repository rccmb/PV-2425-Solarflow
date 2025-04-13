using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarflowServer.DTOs.Hub;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Services;

[ApiExplorerSettings(IgnoreApi = true)]
public class DemoService(ApplicationDbContext context, IEnergyRecordService energyRecordService)
{

    /// <summary>
    /// Simulates energy generation and consumption for all hubs in the system.
    /// It retrieves all hubs and triggers energy iteration for each hub.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DemoEnergy(int minutes = 60)
    {
        var users = await context.Users.ToListAsync();

        foreach (var user in users) await DemoEnergyIteration(user.Id, minutes);
    }

    /// <summary>
    /// Simulates energy generation and consumption for a specific hub.
    /// This method calculates consumption from house, solar, battery, and grid sources.
    /// It also updates the battery charge level and records energy data.
    /// </summary>
    /// <param name="userId">The identifier of the hub for which energy data should be simulated.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DemoEnergyIteration(int userId, int minutes = 60)
    {
        // Hub
        var user = await context.Users.Where(u => u.Id == userId).Include(applicationUser => applicationUser.Battery).FirstOrDefaultAsync();
        if (user == null)
            throw new InvalidOperationException($"Hub with Id {userId} not found.");

        // Timestamp
        var now = DateTime.UtcNow;

        // House
        var house = DemoConsumption(user.GridKWh, now.Hour);

        // Solar
        var solar = DemoSolar(user.SolarKWh, now.Hour);

        // Battery
        var battery = user.Battery;
        var isBatteryChargeForced = battery == null;

        var dto = new EnergyRecordDTO
        {
            ApplicationUserId = user.Id,
            Timestamp = now,
            House = -house,
            Grid = 0.0,
            Solar = solar,
            Battery = 0.0
        };

        // Quotas -----------------------------------------------------------------------------------------

        var quotaConsumption = Math.Abs(house);
        var quotaSolar = Math.Abs(solar);
        var quotaGrid = Math.Abs(user.GridKWh);
        var quotaBatteryCharge = battery?.ChargeLevel ?? 0.0;
        var quotaBatteryDischarge = battery?.ChargeLevel ?? 0.0;

        // House ------------------------------------------------------------------------------------

        // House Consumption from solar
        if (quotaConsumption > 0.0 && quotaSolar > 0.0)
        {
            var usedSolar = Math.Min(quotaConsumption, quotaSolar);
            quotaConsumption -= usedSolar;
            quotaSolar -= usedSolar;
        }

        // House Consumption from battery
        if (quotaConsumption > 0.0 && quotaBatteryDischarge > 0.0)
        {
            var usedBattery = Math.Min(quotaConsumption, quotaBatteryDischarge);
            quotaConsumption -= usedBattery;
            dto.Battery += usedBattery;
        }

        // Consume from grid
        if (quotaConsumption > 0.0)
        {
            var usedGrid = Math.Min(quotaConsumption, quotaGrid);
            quotaConsumption -= usedGrid;
            quotaGrid -= usedGrid;
            dto.Grid += usedGrid;
        }

        // "Trip breaker" if consumption > 0
        if (quotaConsumption > 0.0) throw new Exception();

        // Battery ----------------------------------------------------------------------------------------

        // Charge battery from solar
        if (quotaBatteryCharge > 0.0 && quotaSolar > 0.0)
        {
            var usedSolar = Math.Min(quotaBatteryCharge, quotaSolar);
            quotaSolar -= usedSolar;
            quotaBatteryCharge -= usedSolar;
            dto.Battery -= usedSolar;
        }

        // Charge battery from grid if isForced
        if (quotaBatteryCharge > 0.0 && isBatteryChargeForced && quotaGrid > 0.0)
        {
            var usedGrid = Math.Min(quotaBatteryCharge, quotaGrid);
            dto.Grid += usedGrid;
            dto.Battery -= usedGrid;
        }

        // Grid -------------------------------------------------------------------------------------------

        // Sell remaining solar energy
        if (quotaSolar > 0)
        {
            var usedSolar = quotaSolar;
            dto.Grid -= usedSolar;
        }

        // Update -----------------------------------------------------------------------------------------

        // Update database
        if (battery != null && dto.Battery != 0.0)
        {
            battery.ChargeLevel -= (int)dto.Battery;
            await context.SaveChangesAsync();
        }

        var result = await energyRecordService.AddEnergyRecords(dto);
    }

    /// <summary>
    /// Simulates the total consumption of energy in the house based on the base consumption,
    /// the number of people, and the time of day.
    /// </summary>
    /// <param name="baseConsumptionKWh">The base energy consumption in kWh.</param>
    /// <param name="numberOfPeople">The number of people in the house, used to adjust consumption.</param>
    /// <param name="hour">The current hour of the day used to adjust consumption based on time of day.</param>
    /// <returns>The simulated total energy consumption in kWh.</returns>
    public double DemoConsumption(double gridKWh, int hour, int numberOfPeople = 0)
    {
        // Time Factor
        var timeFactor = hour switch
        {
            0 => 0.3,
            1 => 0.2,
            2 => 0.1,
            3 => 0.1,
            4 => 0.1, // LOW MAX
            5 => 0.2,
            6 => 0.4,
            7 => 0.5,
            8 => 0.6, // HIGH
            9 => 0.6, // HIGH
            10 => 0.5,
            11 => 0.5,
            12 => 0.4,
            13 => 0.4,
            14 => 0.3,
            15 => 0.3, // LOW 
            16 => 0.5,
            17 => 0.6,
            18 => 0.8, // HIGH MAX
            19 => 0.8, // HIGH MAX
            20 => 0.7,
            21 => 0.6,
            22 => 0.5,
            23 => 0.4,
            _ => 0.4
        };

        // People Factor (Low, Medium and High consumption)
        var peopleFactor = numberOfPeople switch
        {
            0 => 1.0,
            1 => 1.1,
            _ => 1.2
        };

        // Random Factor (Make values fluctuate 5%
        var random = new Random();
        var randomFactor = 0.95 + random.NextDouble() * 0.05;

        // Calculate total consumption
        var totalConsumptionKWh = gridKWh * timeFactor * peopleFactor * randomFactor;

        return Math.Round(totalConsumptionKWh, 2);
    }


    /// <summary>
    /// Simulates solar energy production based on the maximum solar capacity, time of day,
    /// and cloud cover factor.
    /// </summary>
    /// <param name="maxCapacityKWh">The maximum solar energy production capacity in kWh.</param>
    /// <param name="hour">The current hour of the day used to calculate solar energy generation.</param>
    /// <param name="cloudFactor">The cloud cover factor, where 0 is completely cloudy and 1 is completely sunny.</param>
    /// <returns>The simulated energy production from solar in kWh.</returns>
    public double DemoSolar(double solarKWh, int hour, double sunFactor = 1)
    {
        // Time Factor
        var timeFactor = hour switch
        {
            7 => 0.1,
            8 => 0.3,
            9 => 0.5,
            10 => 0.6,
            11 => 0.7,
            12 => 0.8,
            13 => 0.9,
            14 => 1.0,
            15 => 0.9,
            16 => 0.8,
            17 => 0.7,
            18 => 0.6,
            19 => 0.5,
            20 => 0.3,
            21 => 0.1,
            _ => 0.0
        };

        // Random Factor (Make values fluctuate 5%
        var random = new Random();
        var randomFactor = 0.95 + random.NextDouble() * 0.05;

        // Energy produced in kWh (max capacity is already in kWh)
        var totalProducedKWh = solarKWh * sunFactor * timeFactor * randomFactor;

        return Math.Round(totalProducedKWh, 2);
    }
}