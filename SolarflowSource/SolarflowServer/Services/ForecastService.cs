﻿using Microsoft.EntityFrameworkCore;
using SolarflowServer.Models;

namespace SolarflowServer.Services;

// Service responsible for retrieving, processing, and storing solar forecasts
public class ForecastService
{
    private readonly ApplicationDbContext _context;
    private readonly WeatherProcessingService _weatherProcessor;
    private readonly WindyApiClient _windyApiClient;

    public ForecastService(WindyApiClient windyApiClient, WeatherProcessingService weatherProcessor,
        ApplicationDbContext context)
    {
        _windyApiClient = windyApiClient;
        _weatherProcessor = weatherProcessor;
        _context = context;
    }

    // Fetches weather forecast and saves energy-related data to the database
    public async Task SaveForecastAsync(int batteryID, double latitude, double longitude, int daysAhead)
    {
        var forecastData = await _windyApiClient.GetWeatherForecastAsync(latitude, longitude);
        var forecasts = forecastData.GetFormattedForecast(daysAhead);

        // Group forecast entries by day
        var groupedByDay = forecasts
            .GroupBy(f => DateTime.Parse(f.DateTime).Date);

        foreach (var dayGroup in groupedByDay)
        {
            // Filter for daytime hours only (06:00 - 18:00)
            var dailyForecasts = dayGroup
                .Where(f =>
                {
                    var hour = DateTime.Parse(f.DateTime).Hour;
                    return hour >= 6 && hour <= 18;
                })
                .ToList();

            if (!dailyForecasts.Any())
                continue;

            // Calculate solar-related metrics from daily data
            var solarHours = _weatherProcessor.CalculateSolarExposure(dailyForecasts);
            var energy = _weatherProcessor.CalculateEnergyGenerated(dailyForecasts);
            var weatherCondition = _weatherProcessor.GetMostCommonWeatherCondition(dailyForecasts);

            await SaveOrUpdateForecast(batteryID, dayGroup.Key, solarHours, weatherCondition, energy);
        }
    }

    // Gets the forecast (in memory only) for 7 days and formats it as Forecast objects
    public async Task<List<Forecast>> GetForecast(double latitude, double longitude)
    {
        var forecastData = await _windyApiClient.GetWeatherForecastAsync(latitude, longitude);
        var forecasts = forecastData.GetFormattedForecast(7);

        var groupedByDay = forecasts
            .GroupBy(f => DateTime.Parse(f.DateTime).Date);

        return (from dayGroup in groupedByDay
                let dailyForecasts = dayGroup
                    .Where(f =>
                    {
                        var hour = DateTime.Parse(f.DateTime).Hour;
                        return hour >= 6 && hour <= 18;
                    })
                    .ToList()
                where dailyForecasts.Any()
                let solarHours = _weatherProcessor.CalculateSolarExposure(dailyForecasts)
                let energy = _weatherProcessor.CalculateEnergyGenerated(dailyForecasts)
                let weatherCondition = _weatherProcessor.GetMostCommonWeatherCondition(dailyForecasts)
                select new Forecast
                {
                    BatteryID = 0, // Not saved to DB
                    ForecastDate = dayGroup.Key,
                    SolarHoursExpected = solarHours,
                    ID = 0,
                    WeatherCondition = weatherCondition,
                    kwh = energy
                }).ToList();
    }

    // Saves or updates forecast data in the database for a specific battery and date
    private async Task SaveOrUpdateForecast(int batteryID, DateTime forecastDate, double solarHours,
        string weatherCondition, double energy)
    {
        var existingForecast = await _context.Forecasts.FirstOrDefaultAsync(f =>
            f.BatteryID == batteryID && f.ForecastDate == forecastDate);

        if (existingForecast == null)
        {
            // Create new forecast entry
            _context.Forecasts.Add(new Forecast
            {
                BatteryID = batteryID,
                ForecastDate = forecastDate,
                SolarHoursExpected = solarHours,
                kwh = energy,
                WeatherCondition = weatherCondition
            });
        }
        else
        {
            // Update existing forecast
            existingForecast.SolarHoursExpected = solarHours;
            existingForecast.WeatherCondition = weatherCondition;
            existingForecast.kwh = energy;

            _context.Forecasts.Update(existingForecast);
        }

        await _context.SaveChangesAsync();
    }

    // Returns current weather conditions and estimated energy generation
    public async Task<CurrentForecastResult?> GetCurrentForecastAsync(double latitude, double longitude)
    {
        var forecastData = await _windyApiClient.GetWeatherForecastAsync(latitude, longitude);
        var forecasts = forecastData.GetFormattedForecast(1);

        // Round current hour to nearest 3-hour block (used by forecast API)
        var now = DateTime.UtcNow;
        var roundedHour = now.Hour - now.Hour % 3;

        var target = forecasts
            .FirstOrDefault(f =>
                DateTimeOffset.Parse(f.DateTime).Hour == roundedHour &&
                DateTimeOffset.Parse(f.DateTime).Date == now.Date);

        if (target == null)
            return null;

        // Estimate solar efficiency and energy output
        var cloudCover = _weatherProcessor.GetAverageCloudCover(target);
        var weatherCondition = _weatherProcessor.GetWeatherCondition(target);
        var efficiency = _weatherProcessor.EvaluateEfficiency(cloudCover);

        // Static configuration: 10 panels, each 450W
        const int numberOfPanels = 10;
        const int panelWattage = 450;
        var totalPanelKW = numberOfPanels * panelWattage / 1000.0;
        var energy = totalPanelKW * efficiency * 3; // 3-hour interval energy

        return new CurrentForecastResult
        {
            Timestamp = DateTimeOffset.Parse(target.DateTime),
            TemperatureC = Math.Round(target.TemperatureCelsius, 1),
            WeatherCondition = weatherCondition,
            EnergyGeneratedKwh = Math.Round(energy, 2)
        };
    }
}

// DTO representing the output of current forecast calculation
public class CurrentForecastResult
{
    public DateTimeOffset Timestamp { get; set; }
    public double TemperatureC { get; set; }
    public string WeatherCondition { get; set; }
    public double EnergyGeneratedKwh { get; set; }
}
