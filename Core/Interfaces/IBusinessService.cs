using Core.Dto;
using Core.Responses;

namespace Core.Interfaces
{
    public interface IBusinessService
    {
        /// <summary>
        /// Register a new business, owned by the current user
        /// </summary>
        /// <param name="dto">The id of the business to link this user to, or null if the user is new</param>
        /// <returns><see cref="ServiceResult{BusinessDto}"/> with the registered business</returns>
        public Task<ServiceResult<BusinessDto>> RegisterBusiness(BusinessDto dto);

        /// <summary>
        /// Gets the business assosciated with this user account.
        /// </summary>
        /// <param name="userId">Unique identifier for the user</param>
        /// <returns><see cref="ServiceResult{BusinessDto}"/> with the business</returns>
        public Task<ServiceResult<BusinessDto>> GetBusinessForUser();
    }
}
