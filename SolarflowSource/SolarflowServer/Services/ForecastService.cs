using Microsoft.EntityFrameworkCore;
using SolarflowServer.Models;

namespace SolarflowServer.Services
{
    public class ForecastService
    {
        private readonly WindyApiClient _windyApiClient;
        private readonly WeatherProcessingService _weatherProcessor;
        private readonly ApplicationDbContext _context;  

        public ForecastService(WindyApiClient windyApiClient, WeatherProcessingService weatherProcessor, ApplicationDbContext context)
        {
            _windyApiClient = windyApiClient;
            _weatherProcessor = weatherProcessor;
            _context = context;
        }

        public async Task SaveForecastAsync(int batteryID, double latitude, double longitude, int daysAhead)
        {
            var forecastData = await _windyApiClient.GetWeatherForecastAsync(latitude, longitude);
            var forecasts = forecastData.GetFormattedForecast(daysAhead);
            double totalSolarHours = 0;
            var weatherConditions = new List<string>();

            foreach (var forecast in forecasts)
            {
                DateTime forecastDate = DateTime.Parse(forecast.DateTime);
                int hour = forecastDate.Hour;

    
                if (hour < 6 || hour > 18)
                    continue;

                double solarHours = _weatherProcessor.CalculateSolarExposure(new List<FormattedForecast> { forecast });
                string weatherCondition = _weatherProcessor.GetWeatherCondition(forecast);

                totalSolarHours += solarHours;
                weatherConditions.Add(weatherCondition);
            }

            if (totalSolarHours > 0)
            {
                var energy = _weatherProcessor.CalculateEnergyGenerated(forecasts);
                DateTime forecastDay = DateTime.Parse(forecasts.First().DateTime).Date;
                string mostCommonCondition = weatherConditions.GroupBy(x => x)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;

                await SaveOrUpdateForecast(batteryID, forecastDay, totalSolarHours, mostCommonCondition, energy);
            }
        }



        private async Task SaveOrUpdateForecast(int batteryID, DateTime forecastDate, double solarHours, string weatherCondition, double energy)
        {
            var existingForecast = await _context.Forecasts.FirstOrDefaultAsync(f => f.BatteryID == batteryID && f.ForecastDate == forecastDate);

            if (existingForecast == null)
            {
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
                existingForecast.SolarHoursExpected = solarHours;
                existingForecast.WeatherCondition = weatherCondition;
                _context.Forecasts.Update(existingForecast);
            }

            await _context.SaveChangesAsync();
        }
    }
}
