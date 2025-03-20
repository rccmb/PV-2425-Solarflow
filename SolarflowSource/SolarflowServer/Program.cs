using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SolarflowServer.Services;
using SolarflowServer;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Configure Email Configuration settings
var emailConfig = builder.Configuration
    .GetSection("EmailConfiguration")
    .Get<EmailConfiguration>();

builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration"));
builder.Services.AddSingleton<EmailConfiguration>(sp => sp.GetRequiredService<IOptions<EmailConfiguration>>().Value);
builder.Services.AddScoped<IEmailSender, EmailSender>();


// DATABASE CONNECTION
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// IDENTITY CONFIGURATION USER
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddIdentityCore<ViewAccount>(options =>
    {
        options.User.RequireUniqueEmail = false; 
    })
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

builder.Services.AddControllers();


// SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
