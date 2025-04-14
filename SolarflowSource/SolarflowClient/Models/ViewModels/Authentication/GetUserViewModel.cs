using Newtonsoft.Json;

namespace SolarflowClient.Models.ViewModels.Authentication;

public class GetUserViewModel
{
    [JsonProperty("fullname")] public string Fullname { get; set; }

    [JsonProperty("photo")] public string Photo { get; set; }

    [JsonProperty("email")] public string Email { get; set; }

    [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; }

    [JsonProperty("hasViewAccount")] public bool HasViewAccount { get; set; }

    [JsonProperty("gridKWh")] public double GridKWh { get; set; }

    [JsonProperty("solarKWh")] public double SolarKWh { get; set; }

    [JsonProperty("latitude")] public double Latitude { get; set; }

    [JsonProperty("longitude")] public double Longitude { get; set; }
}