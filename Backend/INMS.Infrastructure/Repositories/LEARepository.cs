using INMS.Domain.Entities;
using INMS.Domain.Interfaces;
using INMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace INMS.Infrastructure.Repositories
{
    public class LEARepository : ILEARepository
    {
        private readonly AppDbContext _context;

        public LEARepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<LEA>> GetAllAsync()
        {
            return await _context.LEAs
                .Include(l => l.Province)
                .ToListAsync();
        }

        public async Task<LEA?> GetByIdAsync(int id)
        {
            return await _context.LEAs
                .Include(l => l.Province)
                .FirstOrDefaultAsync(l => l.LEAId == id);
        }

        public async Task<LEA> AddAsync(LEA lea)
        {
            _context.LEAs.Add(lea);
            await _context.SaveChangesAsync();
            return lea;
        }

        public async Task<LEA> UpdateAsync(LEA lea)
        {
            _context.LEAs.Update(lea);
            await _context.SaveChangesAsync();
            return lea;
        }

        public async Task DeleteAsync(int id)
        {
            var lea = await _context.LEAs.FindAsync(id);

            if (lea == null)
                throw new Exception("LEA not found");

            _context.LEAs.Remove(lea);
            await _context.SaveChangesAsync();
        }
    }
}
