using SolarflowServer.Services;

public class DemoBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int recurrence = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunDemoEnergyIterationAsync();
            await Task.Delay(TimeSpan.FromMinutes(recurrence), stoppingToken);
        }
    }

    private async Task RunDemoEnergyIterationAsync(int minutes = 60)
    {
        using var scope = serviceProvider.CreateScope();
        var demoService = scope.ServiceProvider.GetRequiredService<DemoService>();
        await demoService.DemoEnergy(minutes);
    }
}