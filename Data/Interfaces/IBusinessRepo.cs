using Data.Entity;

namespace Data.Interfaces
{
    public interface IBusinessRepo
    {
        public Task<bool> RegisterBusiness(string userId, Business business);

        public Task<Business?> GetBusinessForUser(string userId);

        public Task<Business?> GetBusiness(Guid businessGuid);
    }
}
