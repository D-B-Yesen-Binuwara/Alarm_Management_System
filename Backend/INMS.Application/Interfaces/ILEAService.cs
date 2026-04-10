using INMS.Domain.Entities;

namespace INMS.Application.Interfaces
{
    public interface ILEAService
    {
        Task<List<LEA>> GetAllAsync();
        Task<LEA?> GetByIdAsync(int id);
        Task<LEA> CreateAsync(LEA lea);
        Task<LEA> UpdateAsync(int id, LEA lea);
        Task DeleteAsync(int id);
    }
}
