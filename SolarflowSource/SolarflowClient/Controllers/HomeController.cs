using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SolarflowClient.Controllers;

public class HomeController : Controller
{
    private readonly HttpClient _httpClient;

    public HomeController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://localhost:7280/api/");
        // _httpClient.BaseAddress = new Uri("https://solarflowapi.azurewebsites.net/api/auth/"); // CHANGE PRODUCTION.
    }

    public IActionResult Index()
    {
        var token = Request.Cookies["AuthToken"];

        if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Authentication");

        return View();
    }


    public ActionResult ExportCSV()
    {
        // JSON Data
        const string json = @"
        [
            { ""Date"": ""2025-03-16"", ""Value"": -23, ""Source"": ""Gains"" },
            { ""Date"": ""2025-03-16"", ""Value"": -47, ""Source"": ""Consumption"" },
            { ""Date"": ""2025-03-16"", ""Value"": 12, ""Source"": ""Gains"" },
            { ""Date"": ""2025-03-15"", ""Value"": -56, ""Source"": ""Consumption"" },
            { ""Date"": ""2025-03-15"", ""Value"": -5, ""Source"": ""Gains"" },
            { ""Date"": ""2025-03-15"", ""Value"": 20, ""Source"": ""Consumption"" }
        ]";

        // Deserialize JSON to dynamic list
        var records = JsonConvert.DeserializeObject<List<JObject>>(json);

        // Convert to CSV
        var csvData = new StringBuilder();
        csvData.AppendLine("Date,Value,Source"); // Header
        foreach (var record in records) csvData.AppendLine($"{record["Date"]},{record["Value"]},{record["Source"]}");

        // Convert CSV data to a byte array
        var fileBytes = Encoding.UTF8.GetBytes(csvData.ToString());

        // Return CSV file as a download
        return File(fileBytes, "text/csv", "data.csv");
    }


    [HttpGet]
    public IActionResult GetPrevisionChartData()
    {
        // TODO: Get Prevision Data
        const int days = 8;
        var startDate = DateTime.Today;
        var labelDates = Enumerable.Range(0, days)
            .Select(i => startDate.AddDays(i).ToString("dd/MM"))
            .ToArray();
        var random = new Random();
        var valuesPrevision = Enumerable.Range(0, days)
            .Select(_ => random.Next(0, 50))
            .ToArray();


        var chart = new
        {
            type = "line",
            data = new
            {
                labels = labelDates,
                datasets = new[]
                {
                    new
                    {
                        label = "Prevision",
                        data = valuesPrevision,
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

    [HttpGet]
    public IActionResult GetConsumptionChartData()
    {
        // TODO: Get Consumption Data
        const int days = 8;
        var startDate = DateTime.Today;
        var labelDates = Enumerable.Range(0, days)
            .Select(i => startDate.AddDays(-i).ToString("dd/MM"))
            .Reverse()
            .ToArray();
        var random = new Random();
        var valuesGain = Enumerable.Range(0, days)
            .Select(_ => random.Next(0, 50))
            .ToArray();
        var valuesConsumption = Enumerable.Range(0, days)
            .Select(_ => random.Next(-100, 0))
            .ToArray();

        var chart = new
        {
            type = "line",
            data = new
            {
                labels = labelDates,
                datasets = new[]
                {
                    new
                    {
                        label = "Gains",
                        data = valuesGain,
                        fill = true,
                        backgroundColor = "rgba(231,187,65, 0.6)",
                        borderColor = "rgba(231,187,65, 1)",
                        borderWidth = 1
                    },
                    new
                    {
                        label = "Consumption",
                        data = valuesConsumption,
                        fill = true,
                        backgroundColor = "rgba(57,62,65, 0.6)",
                        borderColor = "rgba(57,62,65, 1)",
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

    [HttpGet]
    public IActionResult GetBatteryChartData()
    {
        // TODO: Get Battery Data
        var random = new Random();
        var valuesBattery = random.Next(0, 101);
        ;

        var (backgroundColor, borderColor) = valuesBattery switch
        {
            >= 60 => ("rgba(68, 187, 164, 0.6)", "rgba(68, 187, 164, 1)"),
            >= 40 => ("rgba(243, 146, 55, 0.6)", "rgba(243, 146, 55, 1)"),
            _ => ("rgba(219, 83, 117, 0.6)", "rgba(219, 83, 117, 1)")
        };

        var chart = new
        {
            type = "bar",
            data = new
            {
                labels = new[] { "" },
                datasets = new[]
                {
                    new
                    {
                        label = "Charge",
                        data = new[] { valuesBattery },
                        backgroundColor = new[] { backgroundColor },
                        borderColor = new[] { borderColor },
                        borderWidth = 1
                    }
                }
            },
            options = new
            {
                responsive = true,
                maintainAspectRatio = false,
                plugins = new { legend = new { display = false } },
                scales = new
                {
                    y = new
                    {
                        min = 0,
                        max = 100,
                        ticks = new { stepSize = 10 }
                    }
                }
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
}