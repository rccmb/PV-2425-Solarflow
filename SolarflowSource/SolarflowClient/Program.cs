using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SolarflowClient.Controllers;

var builder = WebApplication.CreateBuilder(args);

// CONTAINER
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Authentication/Login";  // Redirect to login page
        options.LogoutPath = "/Authentication/Logout"; // Redirect to logout
        options.AccessDeniedPath = "/Home/AccessDenied"; // Redirect if unauthorized
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
    name: "default",
    pattern: "{controller=Authentication}/{action=Login}/{id?}");

app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["AuthToken"];
    if (!string.IsNullOrEmpty(token))
    {
        context.Request.Headers.Append("Authorization", "Bearer " + token);
    }
    await next();
});

app.Run();
