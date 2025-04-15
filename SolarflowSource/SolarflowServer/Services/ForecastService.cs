using Microsoft.EntityFrameworkCore;
using SolarflowServer.Models;

namespace SolarflowServer.Services;

/// <summary>
/// Service responsible for retrieving, processing, and storing solar forecasts.
/// It interacts with external weather APIs and stores forecast data in the database.
/// </summary>
public class ForecastService
{
    private readonly ApplicationDbContext _context;
    private readonly WeatherProcessingService _weatherProcessor;
    private readonly WindyApiClient _windyApiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForecastService"/> class.
    /// </summary>
    /// <param name="windyApiClient">The <see cref="WindyApiClient"/> to fetch weather forecast data from the external API.</param>
    /// <param name="weatherProcessor">The <see cref="WeatherProcessingService"/> used to process the weather forecast data.</param>
    /// <param name="context">The <see cref="ApplicationDbContext"/> to interact with the database.</param>
    public ForecastService(WindyApiClient windyApiClient, WeatherProcessingService weatherProcessor,
        ApplicationDbContext context)
    {
        _windyApiClient = windyApiClient;
        _weatherProcessor = weatherProcessor;
        _context = context;
    }

    /// <summary>
    /// Retrieves forecast data from the external API, processes it,
    /// calculates solar-related metrics, and saves the result to the database.
    /// </summary>
    /// <param name="batteryID">The ID of the battery associated with the forecast.</param>
    /// <param name="latitude">The latitude of the user's location.</param>
    /// <param name="longitude">The longitude of the user's location.</param>
    /// <param name="daysAhead">The number of days ahead to retrieve forecasts for.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
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

    /// <summary>
    /// Retrieves and processes the forecast for the next 7 days without saving it to the database.
    /// </summary>
    /// <param name="latitude">The latitude of the user's location.</param>
    /// <param name="longitude">The longitude of the user's location.</param>
    /// <returns>A list of processed forecasts containing solar metrics and weather conditions.</returns>
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

    /// <summary>
    /// Saves or updates forecast data in the database for a specific battery and date.
    /// </summary>
    /// <param name="batteryID">The ID of the battery for which the forecast data is being saved or updated.</param>
    /// <param name="forecastDate">The date of the forecast being saved or updated.</param>
    /// <param name="solarHours">The expected number of solar hours for the forecast date.</param>
    /// <param name="weatherCondition">The most common weather condition for the forecast date.</param>
    /// <param name="energy">The expected energy generation (in kWh) for the forecast date.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SaveOrUpdateForecast(int batteryID, DateTime forecastDate, double solarHours, string weatherCondition, double energy)
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

    /// <summary>
    /// Returns the current weather conditions and estimated energy generation based on the latest forecast.
    /// </summary>
    /// <param name="latitude">The latitude of the location to get the forecast for.</param>
    /// <param name="longitude">The longitude of the location to get the forecast for.</param>
    /// <returns>A task representing the asynchronous operation, containing the current forecast result if available, or null if not.</returns>
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


/// <summary>
/// DTO representing the output of the current forecast calculation.
/// </summary>
public class CurrentForecastResult
{
    /// <summary>
    /// Gets or sets the timestamp of the forecast.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the temperature in Celsius for the forecast.
    /// </summary>
    public double TemperatureC { get; set; }

    /// <summary>
    /// Gets or sets the weather condition for the forecast.
    /// </summary>
    public string WeatherCondition { get; set; }

    /// <summary>
    /// Gets or sets the estimated energy generated in kWh for the forecast period.
    /// </summary>
    public double EnergyGeneratedKwh { get; set; }
}
