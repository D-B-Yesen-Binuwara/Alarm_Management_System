using INMS.Domain.Entities;

namespace INMS.Domain.Interfaces
{
    public interface ILEARepository
    {
        Task<List<LEA>> GetAllAsync();
        Task<LEA?> GetByIdAsync(int id);
        Task<LEA> AddAsync(LEA lea);
        Task<LEA> UpdateAsync(LEA lea);
        Task DeleteAsync(int id);
    }
}
