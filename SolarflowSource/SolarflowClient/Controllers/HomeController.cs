using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SolarflowClient.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SolarflowClient.Controllers;

public class HomeController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public HomeController(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

        _httpClient.BaseAddress = _configuration["Environment"].Equals("Development")
            ? new Uri("https://localhost:7280/api/")
            : new Uri("https://solarflowapi.azurewebsites.net/api/");
    }

    public async Task<IActionResult> Index()
    {
        // Fetch energy records
        var energyRequest = CreateAuthorizedRequest(HttpMethod.Get, "home/consumption");
        var energyResponse = await _httpClient.SendAsync(energyRequest);
        if (!energyResponse.IsSuccessStatusCode) return BadRequest("Failed to fetch energy records from the server.");
        var energyJson = await energyResponse.Content.ReadAsStringAsync();
        var energyRecords = JsonSerializer.Deserialize<List<EnergyRecord>>(energyJson);

        // Fetch battery
        var batteryRequest = CreateAuthorizedRequest(HttpMethod.Get, "Battery/get-battery");
        var batteryResponse = await _httpClient.SendAsync(batteryRequest);
        if (!batteryResponse.IsSuccessStatusCode) return BadRequest("Failed to fetch battery from the server.");
        var batteryJson = await batteryResponse.Content.ReadAsStringAsync();
        Console.WriteLine(batteryJson);
        var battery = JsonSerializer.Deserialize<Battery>(batteryJson);

        // Build the view model
        var viewModel = new HomeViewModel
        {
            EnergyRecord = energyRecords.LastOrDefault(),
            EnergyRecords = energyRecords,
            Battery = battery,
            Forecast = null
        };

        return View(viewModel);
    }

    public async Task<ActionResult> Export()
    {
        // Fetch energy records
        var energyRequest = CreateAuthorizedRequest(HttpMethod.Get, "home/consumption");
        var energyResponse = await _httpClient.SendAsync(energyRequest);
        if (!energyResponse.IsSuccessStatusCode) return BadRequest("Failed to fetch energy records from the server.");
        var energyJson = await energyResponse.Content.ReadAsStringAsync();
        var energyRecords = JsonSerializer.Deserialize<List<EnergyRecord>>(energyJson);

        // Prepare CSV
        var csvData = new StringBuilder();
        csvData.AppendLine("ID,HUB_ID,HOUSE,GRID,SOLAR,BATTERY"); // Header
        foreach (var record in energyRecords)
            csvData.AppendLine(
                $"{record.Id},{record.HubId},{record.House}, {record.Grid}, {record.Solar}, {record.Battery}");
        var fileBytes = Encoding.UTF8.GetBytes(csvData.ToString());

        return File(fileBytes, "text/csv", "data.csv");
    }


    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url)
    {
        var token = Request.Cookies["AuthToken"];
        if (string.IsNullOrEmpty(token)) throw new Exception("Authorization token is missing.");

        // Optionally, validate or inspect the token:
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var requestMessage = new HttpRequestMessage(method, url);
        requestMessage.Headers.Add("Authorization", $"Bearer {token}");
        return requestMessage;
    }

    
    [HttpGet]
    public async Task<ActionResult> GetPrevisionChartData()
    {
        // Receive json from server
        var response = await _httpClient.GetAsync("home/prevision");
        if (!response.IsSuccessStatusCode) return BadRequest("Failed to fetch data from the server.");
        var json = await response.Content.ReadAsStringAsync();

        // Convert json to data
        var data = JsonConvert.DeserializeObject<List<ForecastData>>(json);

        var forecastDate = new List<string>();
        var solarHours = new List<int>();

        foreach (var item in data)
        {
            forecastDate.Add(item.ForecastDate);
            solarHours.Add(item.SolarHoursExpected);
        }

        // Prepare chart using data
        var chart = new
        {
            type = "line",
            data = new
            {
                labels = forecastDate,
                datasets = new[]
                {
                    new
                    {
                        label = "Prevision",
                        data = solarHours,
                        fill = true,
                        backgroundColor = "rgba(231,187,65, 0.6)",
                        borderColor = "rgba(231,187,65, 1)",
                        borderWidth = 1
                    }
                }
            },
            options = new
            {
                responsive = true,
                maintainAspectRatio = false,
                plugins = new { legend = new { display = false } }
            }
        };

        return Json(chart);
    }


    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Cookies["AuthToken"];

        if (!string.IsNullOrEmpty(token))
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "auth/logout");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await _httpClient.SendAsync(request);
        }

        Response.Cookies.Delete("AuthToken");

        return RedirectToAction("Login", "Authentication");
    }


    public IActionResult AccessDenied()
    {
        return View();
    }

    public IActionResult RegisterViewAccount()
    {
        return View("~/Views/Users/RegisterViewAccount.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> RegisterViewAccount(string Password)
    {
        var token = Request.Cookies["AuthToken"];


        var requestData = new
        {
            Password
        };

        var jsonContent =
            new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "auth/register-view")
        {
            Content = jsonContent
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode) return RedirectToAction("Index");

        ViewBag.ErrorMessage = "Error registering the View Account.";
        return View("Index");
    }


    [HttpGet]
    public async Task<ActionResult> GetWeatherImage()
    {
        // Fetch the forecast data from the API
        var response = await _httpClient.GetAsync("home/prevision");
        if (!response.IsSuccessStatusCode) return BadRequest("Failed to fetch data from the server.");

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<List<ForecastData>>(json);

        // Determine the image based on weather condition
        var weatherCondition = data[0].WeatherCondition.ToLower();
        var imageUrl = weatherCondition switch
        {
            "partly cloudy" => "/images/weather/partly_cloudy.png",
            "cloudy" => "/images/weather/cloudy.png",
            "very cloudy" => "/images/weather/very_cloudy.png",
            _ => "/images/weather/clear.png"
        };

        return Json(new { imageUrl });
    }

    public class ForecastData
    {
        public string ForecastDate { get; set; }

        public int SolarHoursExpected { get; set; }

        public string WeatherCondition { get; set; }
    }

    public class ConsumptionData
    {
        public string Date { get; set; }

        public int Consumption { get; set; }

        public int Gain { get; set; }
    }
}