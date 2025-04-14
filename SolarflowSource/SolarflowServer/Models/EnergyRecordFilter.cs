using System.ComponentModel.DataAnnotations;
using SolarflowClient.Models.Enums;

namespace SolarflowClient.Models;

public class EnergyRecordFilter
{
    [Display(Name = "Start Date")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime? StartDate { get; set; }

    [Display(Name = "End Date")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime? EndDate { get; set; }

    public TimeInterval? TimeInterval { get; set; }
}