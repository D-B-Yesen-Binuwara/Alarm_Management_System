namespace INMS.Application.Services;

public class ImpactAnalysisService
{
    public void StartImpactAnalysis(int deviceId)
    {
        Console.WriteLine($"Impact analysis started for device: {deviceId}");

        var graph = BuildGraph();
        TraverseGraph(deviceId, graph);
    }

    private Dictionary<int, List<int>> BuildGraph()
    {
        var graph = new Dictionary<int, List<int>>();

        graph[1] = new List<int> { 2, 3 };
        graph[2] = new List<int> { 4 };
        graph[3] = new List<int>();
        graph[4] = new List<int>();

        return graph;
    }

    private void TraverseGraph(int startNode, Dictionary<int, List<int>> graph)
    {
        if (!graph.ContainsKey(startNode))
        {
            Console.WriteLine("Device not found.");
            return;
        }

        foreach (var connectedDevice in graph[startNode])
        {
            Console.WriteLine($"Impacted device: {connectedDevice}");
        }
    }
}