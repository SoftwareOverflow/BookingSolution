using Data.Entity;
using Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    internal partial class ApplicationDbContext : IBusinessContext
    {
        private DbSet<Business> Businesses { get; set; }


        public async Task<bool> RegisterBusiness(Func<Task<ApplicationUser?>> registerUser, Business business)
        {
            using var transaction = Database.BeginTransaction();

            try
            {
                var user = await registerUser();

                if (user != null)
                {
                    business.Users.Add(user);

                    Businesses.Add(business);
                    await SaveChangesAsync();

                    transaction.Commit();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                // TODO logging
                return false;
            }
        }
    }
}
