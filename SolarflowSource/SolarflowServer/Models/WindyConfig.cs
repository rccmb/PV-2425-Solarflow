namespace SolarflowClient.Models
{
    /// <summary>
    /// Represents the configuration required for accessing Windy API.
    /// </summary>
    public class WindyConfig
    {
        /// <summary>
        /// The API key used to authenticate with the Windy API service.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// The latitude coordinate associated with the user's location.
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// The longitude coordinate associated with the user's location.
        /// </summary>
        public double Lon { get; set; }
    }
}