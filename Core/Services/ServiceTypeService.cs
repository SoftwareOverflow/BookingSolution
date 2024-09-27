using AutoMapper;
using Core.Dto;
using Core.Interfaces;
using Core.Responses;
using Data.Entity;
using Data.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Core.Services
{
    internal class ServiceTypeService : IServiceTypeService
    {
        private readonly IServiceRepo _serviceContext;
        private readonly IBusinessRepo _businessContext;

        private readonly IUserServiceInternal _userService;

        private readonly IMapper _mapper;

        private List<ServiceTypeDto> _serviceTypesCache = [];

        public ServiceTypeService(IServiceRepo serviceContext, IBusinessRepo businessContext, IUserServiceInternal userService, IMapper mapper)
        {
            _serviceContext = serviceContext;
            _businessContext = businessContext;

            _userService = userService;

            _mapper = mapper;
        }

        public async Task<ServiceResult<ServiceTypeDto>> CreateOrUpdateServiceType(ServiceTypeDto serviceType)
        {
            try
            {
                var userId = await _userService.GetUserIdAsync();
                if (userId.IsNullOrEmpty())
                {
                    return new ServiceResult<ServiceTypeDto>(null, ResultType.ClientError, ["Unable to find id for user. Ensure you are logged in."]);
                }

                var business = await _businessContext.GetBusiness();
                if (business == null)
                {
                    return new ServiceResult<ServiceTypeDto>(null, ResultType.ClientError, ["Unable to find business for user. Ensure you have a registered business first."]);
                }

                var entity = _mapper.Map<Service>(serviceType);
                entity.BusinessId = business.Id;

                bool result = false;
                if (serviceType.Guid == Guid.Empty)
                {
                    result = await _serviceContext.Create(entity);
                }
                else
                {
                    result = await _serviceContext.Update(entity);
                }

                if (result)
                {
                    serviceType = _mapper.Map<ServiceTypeDto>(entity);

                    return new ServiceResult<ServiceTypeDto>(serviceType, ResultType.Success);
                }
                else
                {
                    return ServiceResult<ServiceTypeDto>.DefaultServerFailure();
                }
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<ServiceTypeDto>.DefaultServerFailure();
        }

        /// <summary>
        /// Gets all available service types for the business assosciated with this user
        /// </summary>
        /// <returns>The assosciated Services</returns>
        public async Task<ServiceResult<List<ServiceTypeDto>>> GetServiceTypes()
        {
            try
            {
                var userId = await _userService.GetUserIdAsync();
                if (userId.IsNullOrEmpty())
                {
                    return new ServiceResult<List<ServiceTypeDto>>(null, ResultType.ClientError, ["Unable to find id for user. Ensure you are logged in."]);
                }

                var business = await _businessContext.GetBusiness();
                if (business == null)
                {
                    return new ServiceResult<List<ServiceTypeDto>>(null, ResultType.ClientError, ["Unable to find business for user. Ensure you have a registered business first."]);
                }

                var entities = await _serviceContext.GetServices();
                var dtos = _mapper.Map<List<ServiceTypeDto>>(entities);

                // TODO remove cache or keep? Potentially change the edit service page to accept a guid and load from the guid (maybe from cache frist)
                // Cache this information as we have transient service
                _serviceTypesCache = dtos;

                return new ServiceResult<List<ServiceTypeDto>>(dtos, ResultType.Success);

            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<List<ServiceTypeDto>>.DefaultServerFailure();
        }

        public async Task<ServiceResult<bool>> DeleteById(Guid id)
        {
            try
            {
                await _serviceContext.Delete(id);
                return new ServiceResult<bool>(true, ResultType.Success);
            }
            catch (InvalidOperationException)
            {
                return new ServiceResult<bool>(false, ResultType.ClientError);
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<bool>.DefaultServerFailure();
        }
    }
}
