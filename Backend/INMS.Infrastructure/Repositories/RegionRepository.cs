using INMS.Domain.Entities;
using INMS.Domain.Interfaces;
using INMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace INMS.Infrastructure.Repositories
{
    public class RegionRepository : IRegionRepository
    {
        private readonly AppDbContext _context;

        public RegionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Region>> GetAllAsync()
        {
            return await _context.Regions.ToListAsync();
        }

        public async Task<Region?> GetByIdAsync(int id)
        {
            return await _context.Regions.FindAsync(id);
        }

        public async Task<Region> AddAsync(Region region)
        {
            _context.Regions.Add(region);
            await _context.SaveChangesAsync();
            return region;
        }

        public async Task<Region> UpdateAsync(Region region)
        {
            _context.Regions.Update(region);
            await _context.SaveChangesAsync();
            return region;
        }

        public async Task DeleteAsync(int id)
        {
            var region = await _context.Regions.FindAsync(id);
            if (region == null)
                throw new Exception("Region not found");

            _context.Regions.Remove(region);
            await _context.SaveChangesAsync();
        }
    }
}

