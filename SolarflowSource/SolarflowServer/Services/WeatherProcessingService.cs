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

        public double CalculateEnergyGenerated(List<FormattedForecast> forecasts)
        {
            double totalEnergyGenerated = 0;

            int numberOfPanels = 10;
            int panelWattage = 450; 
            double totalPanelKW = (numberOfPanels * panelWattage) / 1000.0; 

            foreach (var forecast in forecasts)
            {
                DateTimeOffset forecastDate = DateTimeOffset.Parse(forecast.DateTime);
                int hour = forecastDate.UtcDateTime.Hour;
                if (hour < 6 || hour > 18) continue;

                double cloudCover = (forecast.HighClouds + forecast.MidClouds + forecast.LowClouds) / 3;
                double efficiency = cloudCover < 20 ? 1.0 : cloudCover < 80 ? 0.5 : 0.1;

                
                double energyThisHour = totalPanelKW * efficiency;
                totalEnergyGenerated += energyThisHour;
            }

            return totalEnergyGenerated;
        }

    }
}