using INMS.Infrastructure.Persistence;
using INMS.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

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

            return _context.Devices
                .Where(d => childIds.Contains(d.DeviceId))
                .ToList();
        }
    }
}