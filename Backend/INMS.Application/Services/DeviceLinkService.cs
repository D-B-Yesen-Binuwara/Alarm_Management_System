using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using INMS.Domain.Enums;
using INMS.Domain.Interfaces;
using INMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace INMS.Application.Services
{
    public class DeviceLinkService : IDeviceLinkService
    {
        private readonly IDeviceLinkRepository _repository;
        private readonly AppDbContext _context;

        public DeviceLinkService(IDeviceLinkRepository repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<DeviceLink> CreateLinkAsync(int parentId, int childId)
        {
            if (parentId == childId)
                throw new Exception("Parent and child cannot be same");

            var parent = await _context.Devices.FindAsync(parentId);
            var child = await _context.Devices.FindAsync(childId);

            if (parent == null || child == null)
                throw new Exception("Device not found");

            // TOPOLOGY VALIDATION
             if (!IsValidTopology(parent.DeviceType, child.DeviceType))
                throw new Exception("Invalid topology: Parent-child relationship not allowed");

            var link = new DeviceLink
            {
                ParentDeviceId = parentId,
                ChildDeviceId = childId,
                LinkStatus = "UP"
            };

            return await _repository.AddAsync(link);
        }

        public async Task<List<DeviceLink>> GetAllLinksAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task DeleteLinkAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        private bool IsValidTopology(DeviceType parentType, DeviceType childType)
        {
            switch (parentType)
            {
                case DeviceType.SLBN:
                    return childType == DeviceType.SLBN
                        || childType == DeviceType.CEAN;

                case DeviceType.CEAN:
                    return childType == DeviceType.MSAN
                        || childType == DeviceType.Customer;

                case DeviceType.MSAN:
                    return childType == DeviceType.Customer;

                default:
                    return false;
            }
        }
    }
}