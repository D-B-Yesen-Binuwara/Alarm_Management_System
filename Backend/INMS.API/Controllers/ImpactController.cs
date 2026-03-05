using Microsoft.AspNetCore.Mvc;
using INMS.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;

namespace INMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImpactController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ImpactController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{deviceId}")]
        public IActionResult GetImpact(int deviceId)
        {
            var links = _context.DeviceLinks.ToList();

            var graph = new Dictionary<int, List<int>>();

            foreach (var link in links)
            {
                if (!graph.ContainsKey(link.ParentDeviceId))
                    graph[link.ParentDeviceId] = new List<int>();

                graph[link.ParentDeviceId].Add(link.ChildDeviceId);
            }

            var impactedDeviceIds = new List<int>();
            var visited = new HashSet<int>();
            var queue = new Queue<int>();

            queue.Enqueue(deviceId);
            visited.Add(deviceId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (graph.ContainsKey(current))
                {
                    foreach (var child in graph[current])
                    {
                        if (!visited.Contains(child))
                        {
                            impactedDeviceIds.Add(child);
                            visited.Add(child);
                            queue.Enqueue(child);
                        }
                    }
                }
            }

            var impactedDevices = _context.Devices
                .Where(d => impactedDeviceIds.Contains(d.DeviceId))
                .Select(d => new
                {
                    deviceName = d.DeviceName,
                    deviceType = d.DeviceType,
                    ip = d.IP,
                    priorityLevel = d.PriorityLevel,
                    leaId = d.LEAId,
                    assignedUserId = d.AssignedUserId
                })
                .ToList();

            var failedDevice = _context.Devices
                .Where(d => d.DeviceId == deviceId)
                .Select(d => d.DeviceName)
                .FirstOrDefault();

            return Ok(new
            {
                failedDevice = failedDevice,
                totalImpacted = impactedDevices.Count,
                impactedDevices = impactedDevices
            });
        }
    }
}