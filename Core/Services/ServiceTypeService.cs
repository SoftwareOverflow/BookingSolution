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
        private IServiceContext ServiceContext;
        private IBusinessContext BusinessContext;

        private UserStateManager UserStateManager;

        private IMapper Mapper;

        private List<ServiceTypeDto> ServiceTypesCache = new List<ServiceTypeDto>();

        public ServiceTypeService(IServiceContext serviceContext, IBusinessContext businessContext, UserStateManager userStateManager, IMapper mapper)
        {
            ServiceContext = serviceContext;
            BusinessContext = businessContext;

            UserStateManager = userStateManager;

            Mapper = mapper;
        }

        public async Task<ServiceResult<ServiceTypeDto>> CreateOrUpdateServiceType(ServiceTypeDto serviceType)
        {
            try
            {
                var userId = UserStateManager.UserId ?? "";

                if (userId.IsNullOrEmpty())
                {
                    return new ServiceResult<ServiceTypeDto>(null, ResultType.ClientError, ["Unable to find userId. Ensure you are logged in."]);
                }

                var business = await BusinessContext.GetBusinessForUser(userId!);
                if (business == null)
                {
                    return new ServiceResult<ServiceTypeDto>(null, ResultType.ClientError, ["No business found for user. Ensure you have a registered business first."]);
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
            catch (Exception ex)
            {
                // TODO loggin
            }

            return ServiceResult<ServiceTypeDto>.DefaultServerFailure();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The assosciated Services</returns>
        public async Task<List<ServiceTypeDto>> GetServiceTypes()
        {
            var results = new List<ServiceTypeDto>
            {
                new() {
                    Name = "Pedicure",
                    Price = 29.99m
                },

                new() {
                    Name = "Manicure",
                    Price = 32.99m
                },

                new() {
                    Name = "Arcyllic Nails (Colours)",
                    Price = 18.99m
                },

                new() {
                    Name = "Acryllic Nails (Designs)",
                    Price = 35.99m
                },

                new() {
                    Name = "Wedding Nails",
                    Price = 59.99m
                }
            };

            await Task.Delay(1500);


            // TODO remove cache or keep? Potentially change the edit service page to accept a guid and load from the guid (maybe from cache frist)
            // Cache this information as we have transient service
            ServiceTypesCache = results;

            return results.OrderBy(x => x.Name).ToList();
        }

        public async Task<ServiceTypeDto?> GetServiceTypeByName(string? name)
        {
            //Attempt to load form the transient service cache.
            try
            {
                var item = ServiceTypesCache.Single(x => x.Name == name);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            // Attempt to load from the database in case the temporary cache is not right
            await Task.Delay(100);

            //Invalid
            return null;
        }
    }
}
