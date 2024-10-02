using Core.Responses;

namespace Core.Interfaces
{
    public interface ISetupService
    {
        Task<ServiceResult<Boolean>> IsAccountCreated();

        Task<ServiceResult<Boolean>> IsBusinessRegistered();

        Task<ServiceResult<Boolean>> IsBillingSetup();

        Task<ServiceResult<Boolean>> IsServiceCreated();
    }
}
