using INMS.Domain.Entities;
using INMS.Domain.Interfaces;
using INMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace INMS.Infrastructure.Repositories
{
    public class ProvinceRepository : IProvinceRepository
    {
        private readonly AppDbContext _context;

        public ProvinceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Province>> GetAllAsync()
        {
            return await _context.Provinces
                .Include(p => p.Region)
                .ToListAsync();
        }

        public async Task<Province?> GetByIdAsync(int id)
        {
            return await _context.Provinces
                .Include(p => p.Region)
                .FirstOrDefaultAsync(p => p.ProvinceId == id);
        }

        public async Task<Province> AddAsync(Province province)
        {
            _context.Provinces.Add(province);
            await _context.SaveChangesAsync();
            return province;
        }

        public async Task<Province> UpdateAsync(Province province)
        {
            _context.Provinces.Update(province);
            await _context.SaveChangesAsync();
            return province;
        }

        public async Task DeleteAsync(int id)
        {
            var province = await _context.Provinces.FindAsync(id);

            if (province == null)
                throw new Exception("Province not found");

            _context.Provinces.Remove(province);
            await _context.SaveChangesAsync();
        }
    }
}