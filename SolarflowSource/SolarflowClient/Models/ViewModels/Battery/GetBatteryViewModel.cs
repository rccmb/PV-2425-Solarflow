using SolarflowClient.Models.Enums;

namespace SolarflowClient.Models.ViewModels.Battery;

public class GetBatteryViewModel
{
    public double CapacityMax { get; set; }
    public double ChargeRate { get; set; }
    public double DischargeRate { get; set; }
    public int ChargeLevel { get; set; }
    public BatterySource ChargeSource { get; set; }
    public BatteryMode ChargeMode { get; set; }
    public int ThresholdMin { get; set; }
    public int ThresholdMax { get; set; }
    public TimeSpan ChargeGridStartTime { get; set; }
    public TimeSpan ChargeGridEndTime { get; set; }
    public DateTime LastUpdate { get; set; }
}