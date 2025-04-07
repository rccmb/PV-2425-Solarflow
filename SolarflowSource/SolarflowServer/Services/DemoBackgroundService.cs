using SolarflowServer.Services;

public class DemoBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunDemoEnergyIterationAsync();
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task RunDemoEnergyIterationAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var demoService = scope.ServiceProvider.GetRequiredService<DemoService>();
        await demoService.DemoEnergy();
    }
}