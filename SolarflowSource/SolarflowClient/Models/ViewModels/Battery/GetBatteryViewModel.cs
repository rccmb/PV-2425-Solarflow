using System.ComponentModel.DataAnnotations;


namespace SolarflowClient.Models.ViewModels.Battery
{
    public class GetBatteryViewModel
    {
        public string ApiKey { get; set; }
        public int ChargeLevel { get; set; }
        public string ChargingMode { get; set; }
        public bool EmergencyMode { get; set; }
        public bool AutoOptimization { get; set; }
        public string LastUpdate { get; set; }
    }
}
