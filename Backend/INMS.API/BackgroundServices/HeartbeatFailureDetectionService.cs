using INMS.Application.Interfaces;

namespace INMS.API.BackgroundServices;

public class HeartbeatFailureDetectionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HeartbeatFailureDetectionService> _logger;
    private const int CheckIntervalSeconds = 30;
    private const int FailureTimeoutSeconds = 90;

    public HeartbeatFailureDetectionService(
        IServiceProvider serviceProvider,
        ILogger<HeartbeatFailureDetectionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Heartbeat Failure Detection Service started");

        await Task.Delay(TimeSpan.FromSeconds(45), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DetectFailures();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during failure detection");
            }

            await Task.Delay(TimeSpan.FromSeconds(CheckIntervalSeconds), stoppingToken);
        }
    }

    private async Task DetectFailures()
    {
        using var scope = _serviceProvider.CreateScope();
        _ = scope.ServiceProvider.GetRequiredService<IImpactAnalysisService>();

        // Placeholder implementation: this solution currently has no Heartbeat entity
        // or simulation event service. Keep the hosted service healthy until those
        // components are added and wired in.
        await Task.CompletedTask;
    }
}
