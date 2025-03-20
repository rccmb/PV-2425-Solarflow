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
                DateTimeOffset forecastDate = DateTimeOffset.Parse(forecast.DateTime);
                double hour = forecastDate.UtcDateTime.Hour;
                if (hour < 6 || hour > 18) continue;

                double cloudCover = (forecast.HighClouds + forecast.MidClouds + forecast.LowClouds) / 3;
                totalSolarHours += cloudCover < 20 ? 3 : cloudCover < 80 ? 1.5 : 0;
            }
            return totalSolarHours;
        }

        public string GetWeatherCondition(FormattedForecast forecast)
        {
            double cloudCoverage = (forecast.HighClouds + forecast.LowClouds + forecast.MidClouds) / 3;
            return cloudCoverage < 20 ? "Clear" : cloudCoverage < 50 ? "Partly Cloudy" : cloudCoverage < 80 ? "Cloudy" : "Very Cloudy";
        }
    }
}