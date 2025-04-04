namespace SolarflowServer.DTOs.Hub;

public class EnergyRecordDTO
{
    public int HubID { get; set; }
    public float Value { get; set; }

    public EnergySource Source { get; set; }

    public EnergySource Target { get; set; }

    public DateTime? Timestamp { get; set; }
}