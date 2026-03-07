using INMS.Infrastructure.Persistence;
using System.Linq;

namespace INMS.Application.Services
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
            var link = _context.DeviceLinks
                .FirstOrDefault(x => x.ChildDeviceId == deviceId);

            if (link == null)
                return deviceId;

            return FindRootCause(link.ParentDeviceId);
        }
    }
}