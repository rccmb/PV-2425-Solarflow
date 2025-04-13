namespace SolarflowServer.DTOs.Hub;

public class EnergyRecordDTO
{
    public int Id { get; set; }

    public int ApplicationUserId { get; set; }

    public DateTime Timestamp { get; set; }

    public double House { get; set; }

    public double Grid { get; set; }

    public double Solar { get; set; }

    public double Battery { get; set; }
}