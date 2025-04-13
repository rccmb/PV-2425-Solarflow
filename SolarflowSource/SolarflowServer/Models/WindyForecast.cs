using System.Text.Json.Serialization;

namespace SolarflowServer.Models
{
    /// <summary>
    /// Represents a formatted weather forecast with detailed cloud and temperature data.
    /// </summary>
    public class FormattedForecast
    {
        /// <summary>
        /// Gets or sets the date and time of the forecast in the format "yyyy-MM-dd HH:mm:ss".
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// Gets or sets the temperature in Celsius.
        /// </summary>
        public double TemperatureCelsius { get; set; }

        /// <summary>
        /// Gets or sets the percentage of high clouds in the sky.
        /// </summary>
        public double HighClouds { get; set; }

        /// <summary>
        /// Gets or sets the percentage of low clouds in the sky.
        /// </summary>
        public double LowClouds { get; set; }

        /// <summary>
        /// Gets or sets the percentage of mid-level clouds in the sky.
        /// </summary>
        public double MidClouds { get; set; }
    }

    /// <summary>
    /// Represents raw weather forecast data retrieved from a weather service.
    /// </summary>
    public class WindyForecast
    {
        /// <summary>
        /// Gets or sets the list of timestamps (in Unix time) for the forecast data.
        /// </summary>
        public List<long> Ts { get; set; }

        /// <summary>
        /// Gets or sets the list of surface temperatures (in Kelvin).
        /// </summary>
        [JsonPropertyName("temp-surface")]
        public List<double> Temperature { get; set; }

        /// <summary>
        /// Gets or sets the list of high cloud coverage percentages.
        /// </summary>
        [JsonPropertyName("hclouds-surface")]
        public List<double> HighClouds { get; set; }

        /// <summary>
        /// Gets or sets the list of low cloud coverage percentages.
        /// </summary>
        [JsonPropertyName("lclouds-surface")]
        public List<double> LowClouds { get; set; }

        /// <summary>
        /// Gets or sets the list of mid-level cloud coverage percentages.
        /// </summary>
        [JsonPropertyName("mclouds-surface")]
        public List<double> MidClouds { get; set; }

        /// <summary>
        /// Converts the raw forecast data into a list of formatted forecasts for a specified number of days ahead.
        /// </summary>
        /// <param name="daysAhead">The number of days ahead to include in the forecast.</param>
        /// <returns>A list of <see cref="FormattedForecast"/> objects containing formatted forecast data.</returns>
        public List<FormattedForecast> GetFormattedForecast(int daysAhead)
        {
            var forecast = new List<FormattedForecast>();
            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTimeOffset maxDate = now.AddDays(daysAhead);

            for (int i = 0; i < Ts.Count; i++)
            {
                DateTimeOffset forecastDate = DateTimeOffset.FromUnixTimeMilliseconds(Ts[i]);
                if (forecastDate > maxDate) break;

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
}