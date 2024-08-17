using Data.Entity;

namespace Data.Interfaces
{
    public interface IBusinessContext
    {
        public Task<bool> RegisterBusiness(string userId, Business business);

        public Task<Business?> GetBusinessForUser(string userId);

    }
}
