namespace SolarflowServer.DTOs
{
    namespace SolarflowServer.DTOs
    {
        public class BatteryDTO
        {
            public string ChargingSource { get; set; }
            public string BatteryMode { get; set; }
            public int MinimalTreshold { get; set; }
            public int MaximumTreshold { get; set; }
            public string SpendingStartTime { get; set; }
            public string SpendingEndTime { get; set; }
        }
    }
}
