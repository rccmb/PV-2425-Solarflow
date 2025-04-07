namespace SolarflowClient.Models;

public class EnergyRecord
{
    public int Id { get; set; }

    public int HubId { get; set; }

    public DateTime Timestamp { get; set; }

    public double House { get; set; }

    public double Grid { get; set; }

    public double Solar { get; set; }

    public double Battery { get; set; }
}