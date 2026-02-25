namespace INMS.Application.Services;

public class CorrelationService
{
    private readonly ImpactAnalysisService _impactService;

    public CorrelationService(ImpactAnalysisService impactService)
    {
        _impactService = impactService;
    }

    public void StartCorrelation(int deviceId)
    {
        Console.WriteLine($"Correlation started for device: {deviceId}");

        _impactService.StartImpactAnalysis(deviceId);
    }
}