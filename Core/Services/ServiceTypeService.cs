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
        private readonly IServiceContext ServiceContext;
        private readonly IBusinessContext BusinessContext;

        private readonly IUserServiceInternal UserService;

        private readonly IMapper Mapper;

        private List<ServiceTypeDto> ServiceTypesCache = new List<ServiceTypeDto>();

        public ServiceTypeService(IServiceContext serviceContext, IBusinessContext businessContext, IUserServiceInternal userService, IMapper mapper)
        {
            ServiceContext = serviceContext;
            BusinessContext = businessContext;

            UserService = userService;

            Mapper = mapper;
        }

        public async Task<ServiceResult<ServiceTypeDto>> CreateOrUpdateServiceType(ServiceTypeDto serviceType)
        {
            try
            {
                var userId = await UserService.GetUserIdAsync();
                if (userId.IsNullOrEmpty())
                {
                    return new ServiceResult<ServiceTypeDto>(null, ResultType.ClientError, ["Unable to find id for user. Ensure you are logged in."]);
                }

                var business = await BusinessContext.GetBusinessForUser(userId);
                if (business == null)
                {
                    return new ServiceResult<ServiceTypeDto>(null, ResultType.ClientError, ["Unable to find business for user. Ensure you have a registered business first."]);
                }

                var entity = Mapper.Map<Service>(serviceType);
                entity.BusinessId = business.Id;

                bool result = false;
                if (serviceType.Guid == Guid.Empty)
                {
                    result = await ServiceContext.Create(userId, entity);
                }
                else
                {
                    result = await ServiceContext.Update(userId, entity);
                }

                if (result)
                {
                    serviceType = Mapper.Map<ServiceTypeDto>(entity);

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
                var userId = await UserService.GetUserIdAsync();
                if (userId.IsNullOrEmpty())
                {
                    return new ServiceResult<List<ServiceTypeDto>>(null, ResultType.ClientError, ["Unable to find id for user. Ensure you are logged in."]);
                }

                var business = await BusinessContext.GetBusinessForUser(userId);
                if (business == null)
                {
                    return new ServiceResult<List<ServiceTypeDto>>(null, ResultType.ClientError, ["Unable to find business for user. Ensure you have a registered business first."]);
                }

                var entities = await ServiceContext.GetAllServicesForUser();
                var dtos = Mapper.Map<List<ServiceTypeDto>>(entities);

                // TODO remove cache or keep? Potentially change the edit service page to accept a guid and load from the guid (maybe from cache frist)
                // Cache this information as we have transient service
                ServiceTypesCache = dtos;

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
                await ServiceContext.Delete(id);
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
