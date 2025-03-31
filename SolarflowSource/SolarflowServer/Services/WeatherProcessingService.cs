using SolarflowServer.Models;

namespace SolarflowServer.Services
{
    public class WeatherProcessingService
    {
        public double CalculateSolarExposure(List<FormattedForecast> forecasts)
        {
            double totalSolarHours = 0;
            foreach (var forecast in forecasts)
            {
                var hour = DateTimeOffset.Parse(forecast.DateTime).Hour;
                if (hour < 6 || hour > 18) continue;

                double cloudCover = GetAverageCloudCover(forecast);
                totalSolarHours += cloudCover < 20 ? 3 : cloudCover < 80 ? 1.5 : 0;
            }
            return totalSolarHours;
        }

        public double GetAverageCloudCover(FormattedForecast forecast)
        {
            return (forecast.HighClouds + forecast.MidClouds + forecast.LowClouds) / 3.0;
        }

        public double EvaluateEfficiency(double cloudCover)
        {
            return cloudCover < 20 ? 1.0 : cloudCover < 80 ? 0.5 : 0.1;
        }

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

        public double CalculateEnergyGenerated(List<FormattedForecast> forecasts)
        {
            double totalEnergyGenerated = 0;

            const int numberOfPanels = 10;
            const int panelWattage = 450;
            double totalPanelKW = (numberOfPanels * panelWattage) / 1000.0;

            foreach (var forecast in forecasts)
            {
                var hour = DateTimeOffset.Parse(forecast.DateTime).Hour;
                if (hour < 6 || hour > 18) continue;

                double cloudCover = GetAverageCloudCover(forecast);
                double efficiency = EvaluateEfficiency(cloudCover);

                totalEnergyGenerated += totalPanelKW * efficiency;
            }

            return totalEnergyGenerated;
        }

        public string GetMostCommonWeatherCondition(List<FormattedForecast> forecasts)
        {
            return forecasts
                .GroupBy(GetWeatherCondition)
                .OrderByDescending(g => g.Count())
                .First().Key;
        }

    }
}
