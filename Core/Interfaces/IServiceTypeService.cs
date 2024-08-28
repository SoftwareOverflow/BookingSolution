using Core.Dto;
using Core.Responses;

namespace Core.Interfaces
{
    public interface IServiceTypeService
    {
        public Task<ServiceResult<List<ServiceTypeDto>>> GetServiceTypes();

        public Task<ServiceResult<ServiceTypeDto>> CreateOrUpdateServiceType(ServiceTypeDto dto);

        /// <summary>
        /// Delete the service by Id
        /// </summary>
        /// <param name="id">Unique identifier for the service to be deleted</param>
        /// <returns><see cref="ServiceResult{bool}"/> indicating if the deletion was succesful</returns>
        public Task<ServiceResult<bool>> DeleteById(Guid id);
    }
}
