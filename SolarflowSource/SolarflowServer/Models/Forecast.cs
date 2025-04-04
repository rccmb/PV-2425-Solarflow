using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarflowServer.Models
{
    public class Forecast
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [ForeignKey("BatteryID")]
        public int BatteryID { get; set; }

        [Required]
        public DateTime ForecastDate { get; set; }

        [Required]
        public double kwh { get; set; }

        [Required]
        public double SolarHoursExpected { get; set; }

        [Required]
        public string WeatherCondition { get; set; }
    }
}