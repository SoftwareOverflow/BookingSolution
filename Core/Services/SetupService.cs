using Core.Interfaces;
using Core.Responses;

namespace Core.Services
{
    internal class SetupService(IUserServiceInternal userService, IBusinessService businessService, IServiceTypeService serviceTypeService) : ISetupService
    {
        private readonly IUserServiceInternal _userService = userService;
        private readonly IBusinessService _businessService = businessService;
        private readonly IServiceTypeService _serviceTypeService = serviceTypeService;

        public async Task<ServiceResult<bool>> IsAccountCreated()
        {
            try
            {
                var userId = await _userService.GetUserIdAsync();
                return new ServiceResult<bool>(!string.IsNullOrEmpty(userId));
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<bool>.DefaultServerFailure();
        }

        public async Task<ServiceResult<bool>> IsBusinessRegistered()
        {
            try
            {
                var businessResult = await _businessService.GetBusinessForUser();
                if (businessResult.IsSuccess && businessResult.Result != null)
                {
                    return new ServiceResult<bool>(true);
                }
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<bool>.DefaultServerFailure();
        }

        public Task<ServiceResult<bool>> IsBillingSetup()
        {
            return Task.FromResult(new ServiceResult<bool>(false));
        }

        public async Task<ServiceResult<bool>> IsServiceCreated()
        {
            try
            {
                var result = await _serviceTypeService.GetServiceTypes();

                if (result.IsSuccess)
                {
                    return new ServiceResult<bool>(result.Result!.Count == 0);
                }
            }
            catch (Exception)
            {

            }

            return ServiceResult<bool>.DefaultServerFailure();
        }
    }
}
