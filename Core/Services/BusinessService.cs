using AutoMapper;
using Core.Dto;
using Core.Interfaces;
using Core.Responses;
using Data.Entity;
using Data.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Core.Services
{
    internal class BusinessService : IBusinessService
    {
        private readonly IBusinessRepo _context;
        private readonly IMapper _mapper;
        private readonly IUserServiceInternal _userService;
        public BusinessService(IBusinessRepo context, IMapper mapper, IUserServiceInternal userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<ServiceResult<BusinessDto>> GetBusinessForUser()
        {
            try
            {
                var userId = await _userService.GetUserIdAsync();
                if (userId.IsNullOrEmpty())
                {
                    return new ServiceResult<BusinessDto>(null, ResultType.ClientError, ["Unable to identify user. Please try again."]);
                }

                var entity = await _context.GetBusiness();
                if (entity == null)
                {
                    // It's possible the user has no business (e.g. for new users)
                    // We can still assume this is a successful request
                    return new ServiceResult<BusinessDto>(null, ResultType.Success);
                }

                var dto = _mapper.Map<BusinessDto>(entity);

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
                var userId = await _userService.GetUserIdAsync();
                if (userId.IsNullOrEmpty())
                {
                    return new ServiceResult<BusinessDto>(null, ResultType.ClientError, ["Unable to identify user. Please try again."]);
                }

                var entity = _mapper.Map<Business>(dto);
                var result = await _context.RegisterBusiness(entity);

                if (result)
                {
                    return new ServiceResult<BusinessDto>(dto);
                }
            }
            catch (InvalidOperationException)
            {
                // TODO logging
            }
            catch (Exception)
            {
                // TODO log properly
            }

            return ServiceResult<BusinessDto>.DefaultServerFailure();
        }
    }
}
