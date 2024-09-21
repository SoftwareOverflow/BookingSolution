using AutoMapper;
using Core.Dto;
using Core.Interfaces;
using Core.Responses;
using Data.Entity;
using Data.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Data;

namespace Core.Services
{
    internal class BusinessService : IBusinessService
    {
        private readonly IBusinessRepo Context;
        private readonly IMapper Mapper;
        private readonly IUserServiceInternal UserService;
        public BusinessService(IBusinessRepo context, IMapper mapper, IUserServiceInternal userService)
        {
            Context = context;
            Mapper = mapper;
            UserService = userService;
        }

        public async Task<ServiceResult<BusinessDto>> GetBusinessForUser()
        {
            try
            {
                var userId = await UserService.GetUserIdAsync();
                if (userId.IsNullOrEmpty())
                {
                    return new ServiceResult<BusinessDto>(null, ResultType.ClientError, ["Unable to identify user. Please try again."]);
                }

                var entity = await Context.GetBusinessForUser(userId!);
                if (entity == null)
                {
                    // It's possible the user has no business (e.g. for new users)
                    // We can still assume this is a successful request
                    return new ServiceResult<BusinessDto>(null, ResultType.Success);
                }

                var dto = Mapper.Map<BusinessDto>(entity);

                return new ServiceResult<BusinessDto>(dto);

            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<BusinessDto>.DefaultServerFailure();

        }

        public async Task<ServiceResult<BusinessDto>> RegisterBusiness(BusinessDto dto)
        {
            try
            {
                var userId = await UserService.GetUserIdAsync();
                if (userId.IsNullOrEmpty())
                {
                    // TODO could this ever be a client error?
                    var failure = ServiceResult<BusinessDto>.DefaultServerFailure();
                    failure.Errors.Add("Unable to find current user");
                    return failure;
                }

                var entity = Mapper.Map<Business>(dto);
                var result = await Context.RegisterBusiness(userId!, entity);

                if (result)
                {
                    return new ServiceResult<BusinessDto>(dto);
                }
            }
            catch (InvalidOperationException ex)
            {
                // TODO logging
                // This would still be a server error as the client should not be able to call this if they have an existing business
            }
            catch (Exception ex)
            {
                // TODO log properly
            }

            return ServiceResult<BusinessDto>.DefaultServerFailure();
        }
    }
}
