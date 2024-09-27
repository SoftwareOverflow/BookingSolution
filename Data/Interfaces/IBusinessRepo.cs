using Data.Entity;

namespace Data.Interfaces
{
    public interface IBusinessRepo
    {
        public Task<bool> RegisterBusiness(Business business);

        public Task<Business?> GetBusiness();
    }
}
