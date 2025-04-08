using Microsoft.AspNetCore.Authentication.Cookies;
using SolarflowClient.Controllers;

var builder = WebApplication.CreateBuilder(args);

// CONTAINER
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    // Set case-insensitive deserialization
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});
;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Authentication/Login";
        options.LogoutPath = "/Authentication/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

// HTTP CLIENT
builder.Services.AddHttpClient<AuthenticationController>();

// SESSION
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

var app = builder.Build();

// HTTP PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Authentication}/{action=Login}/{id?}");

app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["AuthToken"];
    if (!string.IsNullOrEmpty(token)) context.Request.Headers.Append("Authorization", "Bearer " + token);
    await next();
});

app.Run();