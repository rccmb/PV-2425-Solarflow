namespace SolarflowClient.Models
{
    /// <summary>
    /// Represents the configuration settings required for interacting with the Windy API.
    /// </summary>
    public class WindyConfig
    {
        /// <summary>
        /// Gets or sets the API key used for authenticating requests to the Windy API.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the location for which weather data is requested.
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the location for which weather data is requested.
        /// </summary>
        public double Lon { get; set; }
    }
}