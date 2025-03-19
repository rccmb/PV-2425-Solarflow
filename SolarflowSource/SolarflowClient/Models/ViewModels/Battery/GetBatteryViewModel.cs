namespace SolarflowClient.Models.ViewModels.Battery
{
    public class GetBatteryViewModel
    {
        public int ChargeLevel { get; set; }
        public string ChargingSource { get; set; }
        public string BatteryMode { get; set; }
        public int MinimalTreshold { get; set; }
        public int MaximumTreshold { get; set; }
        public string SpendingStartTime { get; set; }
        public string SpendingEndTime { get; set; }
        public string LastUpdate { get; set; }
    }
}
