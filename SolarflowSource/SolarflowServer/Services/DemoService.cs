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
    public async Task DemoEnergy()
    {
        var hubs = await context.Hubs.ToListAsync();

        foreach (var hub in hubs) await DemoEnergyIteration(hub.Id);
    }

    /// <summary>
    /// Simulates energy generation and consumption for a specific hub.
    /// This method calculates consumption from house, solar, battery, and grid sources.
    /// It also updates the battery charge level and records energy data.
    /// </summary>
    /// <param name="hubId">The identifier of the hub for which energy data should be simulated.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DemoEnergyIteration(int hubId)
    {
        // Parameters
        var hub = await context.Hubs.Where(h => h.Id == hubId).FirstOrDefaultAsync();
        if (hub == null)
            throw new InvalidOperationException($"Hub with Id {hubId} not found.");

        var timeZone = GetTimeZoneOffset(hub.Latitude);
        var now = DateTime.Now.AddHours(timeZone);

        const int cloudFactor = 1;

        var house = DemoConsumption(hub.DemoConsumption, hub.DemoPeople, now.Hour);
        var solar = DemoSolar(hub.DemoSolar, now.Hour, cloudFactor);

        var battery = await context.Batteries.Where(b => b.Id == hub.BatteryId).FirstOrDefaultAsync();
        var quotaBatteryCharge = battery?.ChargeLevel ?? 0.0;
        var quotaBatteryDischarge = battery?.ChargeLevel ?? 0.0;
        var isBatteryChargeForced = battery == null;

        var dto = new EnergyRecordDTO
        {
            HubId = hub.Id,
            Timestamp = now,
            House = 0.0,
            Grid = 0.0,
            Solar = 0.0,
            Battery = 0.0
        };

        // Quotas -----------------------------------------------------------------------------------------

        var quotaConsumption = Math.Abs(house);
        var quotaSolar = Math.Abs(solar);
        var quotaGrid = Math.Abs(hub.GridKWh);

        // House ------------------------------------------------------------------------------------

        // House Consumption from solar
        if (quotaConsumption > 0.0 && quotaSolar > 0.0)
        {
            var usedSolar = Math.Min(quotaConsumption, quotaSolar);
            quotaConsumption -= usedSolar;
            quotaSolar -= usedSolar;
            dto.House -= usedSolar;
            dto.Solar += usedSolar;
        }

        // House Consumption from battery
        if (quotaConsumption > 0.0 && quotaBatteryDischarge > 0.0)
        {
            var usedBattery = Math.Min(quotaConsumption, quotaBatteryDischarge);
            quotaConsumption -= usedBattery;
            quotaBatteryDischarge -= usedBattery;
            dto.House -= usedBattery;
            dto.Battery += usedBattery;
        }

        // Consume from grid
        if (quotaConsumption > 0.0)
        {
            var usedGrid = Math.Min(quotaConsumption, quotaGrid);
            quotaConsumption -= usedGrid;
            quotaGrid -= usedGrid;
            dto.House -= usedGrid;
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
            dto.Solar += usedSolar;
            dto.Battery -= usedSolar;
        }

        // Charge battery from grid if isForced
        if (quotaBatteryCharge > 0.0 && isBatteryChargeForced && quotaGrid > 0.0)
        {
            var usedGrid = Math.Min(quotaBatteryCharge, quotaGrid);
            quotaGrid -= usedGrid;
            quotaBatteryCharge -= usedGrid;
            dto.Grid += usedGrid;
            dto.Battery -= usedGrid;
        }

        // Grid -------------------------------------------------------------------------------------------

        // Sell remaining solar energy
        if (quotaSolar > 0)
        {
            var usedSolar = quotaSolar;
            quotaSolar -= usedSolar;
            dto.Solar += quotaSolar;
            dto.Grid -= quotaSolar;
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
    public double DemoConsumption(double baseConsumptionKWh, int numberOfPeople, int hour)
    {
        var timeFactor = hour switch
        {
            >= 0 and < 1 => 1.15,
            >= 1 and < 3 => 1.10,
            7 => 1.20,
            >= 8 and < 17 => 1.40,
            18 => 1.50,
            19 => 1.70,
            20 => 1.80,
            21 => 1.60,
            22 => 1.45,
            23 => 1.30,
            _ => 1.0
        };

        // People multiplier (increase by 5% per person)
        var peopleFactor = 1 + numberOfPeople * 0.05f;

        // Calculate total consumption in kWh
        var totalConsumptionKWh = baseConsumptionKWh * timeFactor * peopleFactor;

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
    public double DemoSolar(double maxCapacityKWh, int hour, double cloudFactor)
    {
        if (hour < 6 || hour > 18 || cloudFactor < 0 || cloudFactor > 1)
            return 0;

        // Calculate irradiance based on time of day (using sine curve for sun intensity)
        var angle = Math.PI * (hour - 6) / 12.0; // Between 6AM and 6PM
        var irradiance = Math.Sin(angle) * 1000; // Max at noon (1000 W/m²)
        irradiance = Math.Max(0, irradiance);

        // Adjust for cloudiness (0 = cloudy, 1 = sunny)
        var adjustedIrradiance = irradiance * cloudFactor;

        // Energy produced in kWh (max capacity is already in kWh)
        var energyProducedKWh = maxCapacityKWh * adjustedIrradiance;

        return Math.Round(energyProducedKWh, 2);
    }

    /// <summary>
    /// Calculates the time zone offset based on the longitude of a location.
    /// The offset is derived by dividing the longitude by 15.
    /// </summary>
    /// <param name="longitude">The longitude of the location for which the time zone offset is calculated.</param>
    /// <returns>The time zone offset in hours.</returns>
    public int GetTimeZoneOffset(double longitude)
    {
        return (int)Math.Round(longitude / 15);
    }
}