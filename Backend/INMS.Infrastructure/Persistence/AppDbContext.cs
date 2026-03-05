using Microsoft.EntityFrameworkCore;
using INMS.Domain.Entities;

namespace INMS.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceLink> DeviceLinks { get; set; }
        public DbSet<Alarm> Alarms { get; set; }
    }
}