using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarflowServer.Models
{
    /// <summary>
    /// Represents a forecast for energy production and weather conditions.
    /// </summary>
    public class Forecast
    {
        /// <summary>
        /// Gets or sets the unique identifier for the forecast.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the battery associated with the forecast.
        /// </summary>
        [ForeignKey("BatteryID")]
        public int BatteryID { get; set; }

        /// <summary>
        /// Gets or sets the date for which the forecast is made.
        /// </summary>
        [Required]
        public DateTime ForecastDate { get; set; }

        /// <summary>
        /// Gets or sets the expected energy production in kilowatt-hours.
        /// </summary>
        [Required]
        public double kwh { get; set; }

        /// <summary>
        /// Gets or sets the expected number of solar hours for the forecast date.
        /// </summary>
        [Required]
        public double SolarHoursExpected { get; set; }

        /// <summary>
        /// Gets or sets the expected weather condition for the forecast date.
        /// </summary>
        [Required]
        public string WeatherCondition { get; set; }
    }
}