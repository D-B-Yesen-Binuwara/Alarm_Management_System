using INMS.Application.DTOs;
using INMS.Application.Interfaces;
using INMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace INMS.Application.Services
{
    public class ImpactService : IImpactService
    {
        private readonly AppDbContext _context;

        public ImpactService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ImpactDeviceDto>> GetImpactedDevices(int deviceId)
        {
            var graph = await BuildGraph();

            var impactedIds = TraverseGraph(graph, deviceId);

            var devices = await _context.Devices
                .Where(d => impactedIds.Contains(d.DeviceId) && d.DeviceId != deviceId)
                .Select(d => new ImpactDeviceDto
                {
                    DeviceId = d.DeviceId,
                    DeviceName = d.DeviceName,
                    DeviceType = d.DeviceType,
                    IP = d.IP,
                    PriorityLevel = d.PriorityLevel,
                    LEAId = d.LEAId,
                    AssignedUserId = d.AssignedUserId
                })
                .ToListAsync();

            return devices;
        }

        private async Task<Dictionary<int, List<int>>> BuildGraph()
        {
            var links = await _context.DeviceLinks.ToListAsync();

            var graph = new Dictionary<int, List<int>>();

            foreach (var link in links)
            {
                if (!graph.ContainsKey(link.ParentDeviceId))
                {
                    graph[link.ParentDeviceId] = new List<int>();
                }

                graph[link.ParentDeviceId].Add(link.ChildDeviceId);
            }

            return graph;
        }

        private List<int> TraverseGraph(Dictionary<int, List<int>> graph, int startNode)
        {
            var visited = new List<int>();
            var queue = new Queue<int>();

            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                if (!visited.Contains(node))
                {
                    visited.Add(node);

                    if (graph.ContainsKey(node))
                    {
                        foreach (var child in graph[node])
                        {
                            queue.Enqueue(child);
                        }
                    }
                }
            }

            return visited;
        }
    }
}