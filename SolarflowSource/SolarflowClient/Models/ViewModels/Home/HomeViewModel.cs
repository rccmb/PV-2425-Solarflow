namespace SolarflowClient.Models;

public class HomeViewModel
{
    public EnergyRecord? EnergyRecord { get; set; }

    public List<EnergyRecord>? EnergyRecords { get; set; }

    public Battery? Battery { get; set; }

    public List<Forecast>? Forecast { get; set; }
}