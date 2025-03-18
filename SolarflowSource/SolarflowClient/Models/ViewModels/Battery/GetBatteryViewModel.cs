using System.ComponentModel.DataAnnotations;


namespace SolarflowClient.Models.ViewModels.Battery
{
    public class GetBatteryViewModel
    {
        public int ID { get; set; } // Matches Battery ID

        public int UserId { get; set; } // Maps to UserId from the Battery model

        public string ApiKey { get; set; }

        public int ChargeLevel { get; set; }

        public string ChargingSource { get; set; } // Changed from ChargingMode to match server model

        public string BatteryMode { get; set; } // Matches BatteryMode

        public int MinimalTreshold { get; set; } // Fixed spelling ("Treshold" → "Threshold")

        public int MaximumTreshold { get; set; }

        public string SpendingStartTime { get; set; } // Added from the Battery model

        public string SpendingEndTime { get; set; } // Added from the Battery model

        public string LastUpdate { get; set; }
    }
}
