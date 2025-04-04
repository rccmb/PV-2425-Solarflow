namespace SolarflowServer.DTOs.Hub;

public class EnergyRecordDTO
{
    public int Id { get; set; }

    public int HubId { get; set; }

    public DateTime Timestamp { get; set; }

    public float Consumption { get; set; }

    public float Grid { get; set; }

    public float Solar { get; set; }

    public float Battery { get; set; }
}