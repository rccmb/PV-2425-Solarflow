using SolarflowServer.Models;

// Service responsible for interpreting and calculating solar forecast metrics from raw weather data
namespace SolarflowServer.Services
{
    /// <summary>
    /// Service responsible for interpreting and calculating solar forecast metrics from raw weather data.
    /// </summary>
    public class WeatherProcessingService
    {
        /// <summary>
        /// Calculates the total solar exposure hours from a list of forecasts based on cloud cover.
        /// Only considers daylight hours (06:00 - 18:00).
        /// </summary>
        /// <param name="forecasts">A list of forecast data containing cloud coverage and other weather information.</param>
        /// <returns>The total number of solar exposure hours based on the forecast data.</returns>
        public double CalculateSolarExposure(List<FormattedForecast> forecasts)
        {
            double totalSolarHours = 0;

            foreach (var forecast in forecasts)
            {
                var hour = DateTimeOffset.Parse(forecast.DateTime).Hour;

                // Only consider daylight hours
                if (hour < 6 || hour > 18) continue;

                double cloudCover = GetAverageCloudCover(forecast);

                // Assign solar hours based on cloud coverage
                totalSolarHours += cloudCover < 20 ? 3 : cloudCover < 80 ? 1.5 : 0;
            }

            return totalSolarHours;
        }

        /// <summary>
        /// Returns the average of high, mid, and low cloud cover for a given forecast.
        /// </summary>
        /// <param name="forecast">A forecast object containing cloud coverage information.</param>
        /// <returns>The average cloud cover percentage.</returns>
        public double GetAverageCloudCover(FormattedForecast forecast)
        {
            return (forecast.HighClouds + forecast.MidClouds + forecast.LowClouds) / 3.0;
        }

        /// <summary>
        /// Returns a solar efficiency coefficient based on cloud cover.
        /// The efficiency is a factor that reduces the total energy generation based on cloud cover.
        /// </summary>
        /// <param name="cloudCover">The average cloud cover percentage.</param>
        /// <returns>A solar efficiency coefficient between 0.1 and 1.0, where lower cloud cover results in higher efficiency.</returns>
        public double EvaluateEfficiency(double cloudCover)
        {
            return cloudCover < 20 ? 1.0 : cloudCover < 80 ? 0.5 : 0.1;
        }

        /// <summary>
        /// Converts cloud cover into a human-readable weather condition string.
        /// </summary>
        /// <param name="forecast">A forecast object containing cloud coverage information.</param>
        /// <returns>A string representing the weather condition (e.g., "Clear", "Partly Cloudy", "Cloudy", "Very Cloudy").</returns>
        public string GetWeatherCondition(FormattedForecast forecast)
        {
            double cloudCoverage = GetAverageCloudCover(forecast);

            return cloudCoverage switch
            {
                < 20 => "Clear",
                < 50 => "Partly Cloudy",
                < 80 => "Cloudy",
                _ => "Very Cloudy"
            };
        }

        /// <summary>
        /// Calculates the total expected energy production from all forecasts for a day, based on cloud cover.
        /// The energy generated is calculated considering the solar panel wattage and efficiency based on cloud cover.
        /// </summary>
        /// <param name="forecasts">A list of forecast data containing cloud coverage and other weather information.</param>
        /// <returns>The total energy generated (in kWh) based on the forecast data.</returns>
        public double CalculateEnergyGenerated(List<FormattedForecast> forecasts)
        {
            double totalEnergyGenerated = 0;

            const int numberOfPanels = 10;       // total solar panels
            const int panelWattage = 450;        // watts per panel
            double totalPanelKW = (numberOfPanels * panelWattage) / 1000.0;

            foreach (var forecast in forecasts)
            {
                var hour = DateTimeOffset.Parse(forecast.DateTime).Hour;
                if (hour < 6 || hour > 18) continue;

                double cloudCover = GetAverageCloudCover(forecast);
                double efficiency = EvaluateEfficiency(cloudCover);

                // Energy per forecasted hour (approximated)
                totalEnergyGenerated += totalPanelKW * efficiency;
            }

            return totalEnergyGenerated;
        }

        /// <summary>
        /// Returns the most frequent weather condition for a given day of forecasts.
        /// </summary>
        /// <param name="forecasts">A list of forecast data for a specific day.</param>
        /// <returns>The most common weather condition for the day (e.g., "Clear", "Partly Cloudy", "Cloudy", "Very Cloudy").</returns>
        public string GetMostCommonWeatherCondition(List<FormattedForecast> forecasts)
        {
            return forecasts
                .GroupBy(GetWeatherCondition)
                .OrderByDescending(g => g.Count())
                .First().Key;
        }
    }
}
