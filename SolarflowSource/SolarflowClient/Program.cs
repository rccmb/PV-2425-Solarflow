using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SolarflowClient.Controllers;

var builder = WebApplication.CreateBuilder(args);

// CONTAINER
builder.Services.AddControllersWithViews();

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

app.Run();
