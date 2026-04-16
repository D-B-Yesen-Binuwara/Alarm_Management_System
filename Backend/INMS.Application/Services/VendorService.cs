using INMS.Application.DTOs;
using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using INMS.Domain.Enums;
using INMS.Domain.Interfaces;

namespace INMS.Application.Services;

public class VendorService : IVendorService
{
    private readonly IVendorRepository _vendorRepository;

    public VendorService(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<IEnumerable<Vendor>> GetAllAsync()
    {
        return await _vendorRepository.GetAllAsync();
    }

    public async Task<Vendor?> GetByIdAsync(int id)
    {
        return await _vendorRepository.GetByIdAsync(id);
    }

    public async Task<Vendor> CreateAsync(CreateVendorDto dto)
    {
        var vendor = new Vendor
        {
            Name = dto.Name,
            Brand = dto.Brand,
            DeviceType = dto.DeviceType,
            Description = dto.Description,
            CreatedAt = DateTime.Now,
            IsActive = true
        };

        await _vendorRepository.AddAsync(vendor);
        return vendor;
    }

    public async Task<Vendor?> UpdateAsync(int id, UpdateVendorDto dto)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor == null) return null;

        vendor.Name = dto.Name;
        vendor.Brand = dto.Brand;
        vendor.DeviceType = dto.DeviceType;
        vendor.Description = dto.Description;
        vendor.IsActive = dto.IsActive;

        await _vendorRepository.UpdateAsync(vendor);
        return vendor;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor == null) return false;

        await _vendorRepository.DeleteAsync(vendor);
        return true;
    }

    public async Task<IEnumerable<Vendor>> GetByDeviceTypeAsync(DeviceType deviceType)
    {
        return await _vendorRepository.GetByDeviceTypeAsync(deviceType);
    }

    public async Task<IEnumerable<Vendor>> GetByBrandAsync(string brand)
    {
        return await _vendorRepository.GetByBrandAsync(brand);
    }
}