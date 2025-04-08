using SolarflowServer.Services;

/// <summary>
/// Background service that runs demo energy iterations at regular intervals.
/// </summary>
public class DemoBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    /// <summary>
    /// Executes the background service to run demo energy iterations at regular intervals.
    /// </summary>
    /// <param name="stoppingToken">A token that can be used to signal the cancellation of the task.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunDemoEnergyIterationAsync();
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    /// <summary>
    /// Runs a single demo energy iteration using the demo service.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RunDemoEnergyIterationAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var demoService = scope.ServiceProvider.GetRequiredService<DemoService>();
        await demoService.DemoEnergy();
    }
}