using INMS.Infrastructure.Persistence;
using INMS.Domain.Entities;

namespace INMS.Infrastructure.Services
{
    public class ImpactService
    {
        private readonly AppDbContext _context;

        public ImpactService(AppDbContext context)
        {
            _context = context;
        }

        public List<Device> GetDirectImpacts(int rootDeviceId)
        {
            var childIds = _context.DeviceLinks
                .Where(x => x.ParentDeviceId == rootDeviceId)
                .Select(x => x.ChildDeviceId)
                .ToList();

            var impactedDevices = _context.Devices
                .Where(d => childIds.Contains(d.DeviceId))
                .ToList();

            return impactedDevices;
        }
    }
}