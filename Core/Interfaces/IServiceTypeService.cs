using Core.Dto;

namespace Core.Interfaces
{
    public interface IServiceTypeService
    {
        public Task<List<ServiceTypeDto>> GetServiceTypes();

        public Task<ServiceTypeDto?> GetServiceTypeByName(string? name);
    }
}
