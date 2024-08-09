using Data.Entity;
using Microsoft.AspNetCore.Identity;

namespace Data.Interfaces
{
    public interface IBusinessContext
    {
        public Task<bool> RegisterBusiness(string userId, Business business);

    }
}
