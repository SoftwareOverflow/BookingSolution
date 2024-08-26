using Core.Dto;
using Core.Responses;

namespace Core.Interfaces
{
    public interface IServiceTypeService
    {
        public Task<List<ServiceTypeDto>> GetServiceTypes();

        public Task<ServiceResult<ServiceTypeDto>> CreateOrUpdateServiceType(ServiceTypeDto dto);
    }
}
