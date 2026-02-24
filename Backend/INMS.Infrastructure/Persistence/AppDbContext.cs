using Microsoft.EntityFrameworkCore;
using INMS.Domain.Entities;

namespace INMS.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Device> Devices { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<Province> Provinces { get; set; }
    public DbSet<LEA> LEAs { get; set; }
    public DbSet<Alarm> Alarms { get; set; }
    public DbSet<DeviceLink> DeviceLinks { get; set; }
    public DbSet<RootCause> RootCauses { get; set; }
    public DbSet<ImpactedDevice> ImpactedDevices { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
}
