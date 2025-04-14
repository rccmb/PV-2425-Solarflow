using SolarflowClient.Models.ViewModels;

namespace SolarflowClient.Models;

public class HomeViewModel
{
    public EnergyRecord? LastEnergyRecord { get; set; }

    public List<EnergyRecord>? EnergyRecords { get; set; }

    public Battery? Battery { get; set; }

    public List<Forecast>? Forecast { get; set; }

    public EnergyRecordFilter Filter { get; set; } = new EnergyRecordFilter();
}