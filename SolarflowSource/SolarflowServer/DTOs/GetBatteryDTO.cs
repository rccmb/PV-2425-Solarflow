namespace SolarflowServer.DTOs
{
    namespace SolarflowServer.DTOs
    {
        public class GetBatteryDto
        {
            public int ID { get; set; }
            public int UserId { get; set; }
            public string ApiKey { get; set; }
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

}
