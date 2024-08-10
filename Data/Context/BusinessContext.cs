using Data.Entity;
using Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    internal partial class ApplicationDbContext : IBusinessContext
    {
        private DbSet<Business> Businesses { get; set; }

        public async Task<bool> RegisterBusiness(string userId, Business business)
        {
            using var transaction = Database.BeginTransaction();

            try
            {
                /*var user = await Users.SingleAsync(x => x.Id == userId);

                if (user != null)
                {
                    Businesses.Add(business);
                    await SaveChangesAsync();

                    user.BusinessId = business.Id;
                    await SaveChangesAsync();

                    transaction.Commit();

                    return true;
                }*/

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
