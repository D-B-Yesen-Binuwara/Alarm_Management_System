using System.Linq;
using INMS.Infrastructure.Persistence;

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
            int current = deviceId;

            while (true)
            {
                var parent = _context.DeviceLinks
                    .FirstOrDefault(x => x.ChildDeviceId == current);

                if (parent == null)
                    break;

                current = parent.ParentDeviceId;
            }

            return current;
        }
    }
}