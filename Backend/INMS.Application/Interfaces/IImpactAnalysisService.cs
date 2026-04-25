namespace INMS.Application.Interfaces;

public interface IImpactAnalysisService
{
    Task AnalyzeFailureAsync(int deviceId);
    Task ClearRootCauseAsync(int deviceId);
    Task ClearImpactAsync(int deviceId);
}
