using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SolarflowServer.Controllers;
using SolarflowServer.Models;
using SolarflowServer.Services;
using SolarflowServer.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configure Email Configuration settings
var emailConfig = builder.Configuration
    .GetSection("EmailConfiguration")
    .Get<EmailConfiguration>();

builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration"));
builder.Services.AddSingleton<EmailConfiguration>(sp => sp.GetRequiredService<IOptions<EmailConfiguration>>().Value);
builder.Services.AddScoped<EmailSender>();
builder.Services.AddScoped<DemoService>();

// DATABASE CONNECTION
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// IDENTITY CONFIGURATION USER
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<int>>() // Add Roles
    .AddRoles<IdentityRole<int>>() // Ensure Role Support
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddIdentityCore<ViewAccount>(options => { options.User.RequireUniqueEmail = false; })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<SignInManager<ViewAccount>>();

// JWT AUTHENTICATION
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
            ValidateIssuer = false,
            ValidateAudience = false
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("JWT Authentication failed: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("JWT Token validated successfully.");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddAuthorization();

builder.Services.AddHttpClient<WindyApiClient>();
builder.Services.AddScoped<WeatherProcessingService>();
builder.Services.AddScoped<ForecastService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISuggestionService, SuggestionService>();


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


// SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IEnergyRecordService, EnergyRecordService>();


// Register the DemoBackgroundService to run periodically
builder.Services.AddHostedService<DemoBackgroundService>();

var app = builder.Build();

// HTTP PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();