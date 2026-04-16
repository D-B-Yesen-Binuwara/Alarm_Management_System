using INMS.Domain.Entities;
using INMS.Domain.Enums;
using INMS.Application.DTOs;

namespace INMS.Application.Interfaces
{
    public interface IVendorService
    {
        Task<IEnumerable<Vendor>> GetAllAsync();
        Task<Vendor?> GetByIdAsync(int id);
        Task<Vendor> CreateAsync(CreateVendorDto dto);
        Task<Vendor?> UpdateAsync(int id, UpdateVendorDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Vendor>> GetByDeviceTypeAsync(DeviceType deviceType);
        Task<IEnumerable<Vendor>> GetByBrandAsync(string brand);
    }
}
