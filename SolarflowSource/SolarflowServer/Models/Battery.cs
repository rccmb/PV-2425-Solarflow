using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarflowServer.Models
{
    public class Battery
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int ChargeLevel { get; set; }

        public string ChargingSource { get; set; }

        public string BatteryMode { get; set; }

        public int MinimalTreshold { get; set; }

        public int MaximumTreshold { get; set; }

        public string SpendingStartTime { get; set; }

        public string SpendingEndTime { get; set; }

        public string LastUpdate { get; set; }

        [ForeignKey("ApplicationUser")]
        public int UserId { get; set; }

        public ApplicationUser User { get; set; }
    }
}
