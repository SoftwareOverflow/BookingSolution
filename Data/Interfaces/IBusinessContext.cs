using Data.Entity;
using Microsoft.AspNetCore.Identity;

namespace Data.Interfaces
{
    public interface IBusinessContext
    {
        public Task<bool> RegisterBusiness(Func<Task<ApplicationUser?>> registerUser, Business business);

    }
}
