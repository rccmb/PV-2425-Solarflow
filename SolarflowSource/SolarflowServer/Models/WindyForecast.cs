using System.Text.Json.Serialization;

namespace SolarflowServer.Models
{
    public class WindyForecast
    {
        public List<long> Ts { get; set; }

        [JsonPropertyName("temp-surface")]
        public List<double> Temperature { get; set; }

        [JsonPropertyName("hclouds-surface")]
        public List<double> HighClouds { get; set; }

        [JsonPropertyName("lclouds-surface")]
        public List<double> LowClouds { get; set; }

        [JsonPropertyName("mclouds-surface")]
        public List<double> MidClouds { get; set; }

        public List<FormattedForecast> GetFormattedForecast(int daysAhead)
        {
            var forecast = new List<FormattedForecast>();

            if (daysAhead > 10 && daysAhead <= 0)
            {
                throw new ArgumentException("The number of days must be between 1 and 10");
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTimeOffset maxDate = now.AddDays(daysAhead);

            for (int i = 0; i < Ts.Count; i++)
            {
                DateTimeOffset forecastDate = DateTimeOffset.FromUnixTimeMilliseconds(Ts[i]);

                if (forecastDate > maxDate)
                    break; 

                forecast.Add(new FormattedForecast
                {
                    DateTime = forecastDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    TemperatureCelsius = Temperature[i] - 273.15,
                    HighClouds = HighClouds[i],
                    LowClouds = LowClouds[i],
                    MidClouds = MidClouds[i]
                });
            }

            return forecast;
        }
    }

    public class FormattedForecast
    {
        public string DateTime { get; set; }
        public double TemperatureCelsius { get; set; }
        public double HighClouds { get; set; }
        public double LowClouds { get; set; }
        public double MidClouds { get; set; }
    }
    public class SolarForecast
    {
        public List<FormattedForecast> Forecasts { get; set; }
        public double SolarHours { get; set; }
    }
}