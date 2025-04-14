using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SolarflowClient.Models;
using SolarflowClient.Models.Enums;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SolarflowClient.Controllers;

/// <summary>
///     Controller responsible for handling the main dashboard logic, energy data retrieval,
///     export functionality, weather forecasts, battery data, and user-specific actions.
/// </summary>
public class HomeController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HomeController" /> class.
    ///     Sets the API base address based on the application environment.
    /// </summary>
    /// <param name="httpClient">HTTP client used for API communication.</param>
    /// <param name="configuration">Application configuration service.</param>
    public HomeController(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

        _httpClient.BaseAddress = _configuration["Environment"].Equals("Development")
            ? new Uri("https://localhost:7280/api/")
            : new Uri("https://solarflowapi.azurewebsites.net/api/");
    }

    /// <summary>
    ///     Displays the main dashboard with energy, battery, and forecast data for a given date range.
    /// </summary>
    /// <returns>The dashboard view populated with retrieved data or a redirect to login if unauthorized.</returns>
    public async Task<IActionResult> Index(HomeViewModel model)
    {

        var token = Request.Cookies["AuthToken"];
        if (string.IsNullOrEmpty(token))
        {
            TempData["ErrorMessage"] = "You must be logged in to update your account.";
            return RedirectToAction("Login", "Authentication");
        }

        var startDate = model.Filter.StartDate ?? DateTime.Today;
        var endDate = model.Filter.EndDate ?? DateTime.Today.AddDays(1);
        var timeInterval = model.Filter.TimeInterval ?? TimeInterval.Minute;


        var energyRecordLast = await GetEnergyRecordLast();
        var energyRecords = await GetEnergyRecords(startDate, endDate, timeInterval);
        var forecast = await GetForecasts();
        var battery = await GetBattery();

        // Filter
        var filter = new EnergyRecordFilter
        {
            StartDate = startDate,
            EndDate = endDate,
            TimeInterval = timeInterval
        };


        // View Model
        var viewModel = new HomeViewModel
        {
            LastEnergyRecord = energyRecordLast,
            EnergyRecords = energyRecords,
            Battery = battery,
            Forecast = forecast,
            Filter = filter
        };

        return View(viewModel);
    }

    private async Task<EnergyRecord?> GetEnergyRecordLast()
    {
        const string urlLast = "home/last";
        var requestLast = CreateAuthorizedRequest(HttpMethod.Get, urlLast);
        var responseLast = await _httpClient.SendAsync(requestLast);
        EnergyRecord energyRecordLast = new();

        if (!responseLast.IsSuccessStatusCode) return energyRecordLast;
        var jsonLast = await responseLast.Content.ReadAsStringAsync();
        energyRecordLast = JsonSerializer.Deserialize<EnergyRecord>(jsonLast);

        return energyRecordLast;
    }

    private async Task<List<EnergyRecord>> GetEnergyRecords(DateTime? startDate, DateTime? endDate,
        TimeInterval? timeInterval)
    {
        var urlRecords =
            $"home/records?startDate={startDate:s}&endDate={endDate:s}&timeInterval={timeInterval}";
        var requestRecords = CreateAuthorizedRequest(HttpMethod.Get, urlRecords);
        var responseRecords = await _httpClient.SendAsync(requestRecords);
        List<EnergyRecord>? energyRecords = [];

        if (!responseRecords.IsSuccessStatusCode) return energyRecords;
        var jsonRecords = await responseRecords.Content.ReadAsStringAsync();
        energyRecords = JsonSerializer.Deserialize<List<EnergyRecord>>(jsonRecords);
        energyRecords = energyRecords.OrderBy(x => x.Timestamp).ToList();

        return energyRecords;
    }

    private async Task<List<Forecast>?> GetForecasts()
    {
        // Forecast
        const string urlForecast = "home/prevision";
        var requestForecast = CreateAuthorizedRequest(HttpMethod.Get, urlForecast);
        var responseForecast = await _httpClient.SendAsync(requestForecast);
        List<Forecast>? forecast = [];


        if (!responseForecast.IsSuccessStatusCode) return forecast;
        var jsonForecast = await responseForecast.Content.ReadAsStringAsync();
        forecast = JsonSerializer.Deserialize<List<Forecast>>(jsonForecast);

        return forecast;
    }

    private async Task<Battery?> GetBattery()
    {
        // Battery
        const string urlBattery = "Battery/get-battery";
        var requestBattery = CreateAuthorizedRequest(HttpMethod.Get, urlBattery);
        var responseBattery = await _httpClient.SendAsync(requestBattery);
        Battery battery = null;


        if (!responseBattery.IsSuccessStatusCode) return battery;
        var jsonBattery = await responseBattery.Content.ReadAsStringAsync();
        battery = JsonSerializer.Deserialize<Battery>(jsonBattery);

        return battery;
    }


    /// <summary>
    ///     Aggregates a collection of energy records based on the specified time interval.
    ///     Uses averages for minute and hour intervals, and sums for other intervals.
    /// </summary>
    /// <param name="records">List of EnergyRecord objects.</param>
    /// <param name="interval">Grouping time interval.</param>
    /// <returns>Aggregated collection of EnergyRecord objects.</returns>
    /// <summary>
    ///     Exports all energy records in CSV format.
    /// </summary>
    /// <returns>A downloadable CSV file containing energy record data.</returns>
    public async Task<ActionResult> ssExport()
    {
        // Fetch energy records
        var energyRequest = CreateAuthorizedRequest(HttpMethod.Get, "home/records");
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


    public async Task<IActionResult> Export([FromQuery] EnergyRecordFilter filter)
    {
        var startDate = filter.StartDate;
        var endDate = filter.EndDate;
        var timeInterval = filter.TimeInterval;

        Console.WriteLine("StartDate: " + startDate);
        Console.WriteLine("EndDate: " + endDate);
        Console.WriteLine("TimeInterval: " + timeInterval);

        var records = await GetEnergyRecords(startDate, endDate, timeInterval);

        var csvData = new StringBuilder();
        csvData.AppendLine("Timestamp,HUB_ID,HOUSE,GRID,SOLAR,BATTERY");
        foreach (var record in records)
        {
            Console.WriteLine(record);
            csvData.AppendLine($"{record.Timestamp},{record.HubId},{record.House},{record.Grid},{record.Solar},{record.Battery}");
        }

        var fileBytes = Encoding.UTF8.GetBytes(csvData.ToString());
        return File(fileBytes, "text/csv", "export.csv");
    }

    /// <summary>
    ///     Helper method that builds an authorized HTTP request using the token from cookies.
    /// </summary>
    /// <param name="method">HTTP method to use (GET, POST, etc.).</param>
    /// <param name="url">Endpoint URL (relative to base address).</param>
    /// <returns>A prepared <see cref="HttpRequestMessage" /> with an Authorization header.</returns>
    /// <exception cref="Exception">Thrown if no token is found in the cookies.</exception>
    private HttpRequestMessage? CreateAuthorizedRequest(HttpMethod method, string url)
    {
        var token = Request.Cookies["AuthToken"];
        if (string.IsNullOrEmpty(token))
        {
            Response.Redirect("/Authentication/Login", true);
            return null;
        }

        // Optionally, validate or inspect the token:
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var requestMessage = new HttpRequestMessage(method, url);
        requestMessage.Headers.Add("Authorization", $"Bearer {token}");
        return requestMessage;
    }

    /// <summary>
    ///     Logs the current user out by invalidating the token and clearing cookies.
    /// </summary>
    /// <returns>Redirects the user to the login page.</returns>
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

    /// <summary>
    ///     Displays an access denied view.
    /// </summary>
    /// <returns>The access denied view.</returns>
    public IActionResult AccessDenied()
    {
        return View();
    }

    /// <summary>
    ///     Displays the register view account form.
    /// </summary>
    /// <returns>The register view account page.</returns>
    public IActionResult RegisterViewAccount()
    {
        return View("~/Views/Users/RegisterViewAccount.cshtml");
    }

    /// <summary>
    ///     Registers a new view-only user account using a provided password.
    /// </summary>
    /// <param name="Password">The password for the new view account.</param>
    /// <returns>Redirects to the index page on success; otherwise, displays an error.</returns>
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

    /// <summary>
    ///     Determines and returns the appropriate weather image URL based on the latest forecast.
    /// </summary>
    /// <returns>A JSON object containing the weather image URL, or a BadRequest on failure.</returns>
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

    /// <summary>
    ///     Represents weather forecast data returned from the API.
    /// </summary>
    public class ForecastData
    {
        /// <summary>
        ///     Gets or sets the forecast date.
        /// </summary>
        public string ForecastDate { get; set; }

        /// <summary>
        ///     Gets or sets the expected number of solar hours.
        /// </summary>
        public int SolarHoursExpected { get; set; }

        /// <summary>
        ///     Gets or sets the weather condition description.
        /// </summary>
        public string WeatherCondition { get; set; }
    }

    /// <summary>
    ///     Represents a record of energy records and gain for a given day.
    /// </summary>
    public class ConsumptionData
    {
        /// <summary>
        ///     Gets or sets the date of the record.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        ///     Gets or sets the energy records value.
        /// </summary>
        public int Consumption { get; set; }

        /// <summary>
        ///     Gets or sets the energy gain value.
        /// </summary>
        public int Gain { get; set; }
    }
}