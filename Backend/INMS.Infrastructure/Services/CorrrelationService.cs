using INMS.Infrastructure.Persistence;
using INMS.Domain.Entities;
using System.Linq;

namespace INMS.Infrastructure.Services
{
    public class CorrelationService
    {
        private readonly AppDbContext _context;

        public CorrelationService(AppDbContext context)
        {
            _context = context;
        }

        public int FindRootCause(int deviceId)
        {
            var parentLink = _context.DeviceLinks
                .FirstOrDefault(x => x.ChildDeviceId == deviceId);

            if (parentLink == null)
                return deviceId;

            var parentDevice = _context.Devices
                .FirstOrDefault(d => d.DeviceId == parentLink.ParentDeviceId);

            if (parentDevice == null)
                return deviceId;

            if (parentDevice.Status == "DOWN")
                return FindRootCause(parentDevice.DeviceId);

            return deviceId;
        }
    }
}