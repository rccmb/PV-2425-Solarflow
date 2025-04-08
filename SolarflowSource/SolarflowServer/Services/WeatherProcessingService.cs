using SolarflowServer.Models;

// Service responsible for interpreting and calculating solar forecast metrics from raw weather data
namespace SolarflowServer.Services
{
    public class WeatherProcessingService
    {
        // Calculates total solar exposure hours from a list of forecasts, based on cloud cover
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

        // Returns the average of high, mid and low cloud cover for a forecast
        public double GetAverageCloudCover(FormattedForecast forecast)
        {
            return (forecast.HighClouds + forecast.MidClouds + forecast.LowClouds) / 3.0;
        }

        // Returns a solar efficiency coefficient based on cloud cover
        public double EvaluateEfficiency(double cloudCover)
        {
            return cloudCover < 20 ? 1.0 : cloudCover < 80 ? 0.5 : 0.1;
        }

        // Converts cloud cover into a human-readable weather condition string
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

        // Calculates total expected energy production from all forecasts for a day
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

        // Returns the most frequent weather condition for a given day of forecasts
        public string GetMostCommonWeatherCondition(List<FormattedForecast> forecasts)
        {
            return forecasts
                .GroupBy(GetWeatherCondition)
                .OrderByDescending(g => g.Count())
                .First().Key;
        }
    }
}
