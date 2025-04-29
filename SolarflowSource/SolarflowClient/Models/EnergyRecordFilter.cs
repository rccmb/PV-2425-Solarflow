using System.ComponentModel.DataAnnotations;
using SolarflowClient.Models.Enums;

namespace SolarflowClient.Models;

/// <summary>
/// Represents a filter for querying energy records based on date range and time interval.
/// </summary>
public class EnergyRecordFilter
{
    /// <summary>
    /// Gets or sets the start date for the energy record query.
    /// </summary>
    [Display(Name = "StartDate")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date for the energy record query.
    /// </summary>
    [Display(Name = "EndDate")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the time interval for grouping energy records.
    /// </summary>
    public TimeInterval? TimeInterval { get; set; }
}