using Data.Entity;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    internal partial class ApplicationDbContext : IBusinessContext
    {
        private DbSet<Business> Businesses { get; set; }

        private DbSet<BusinessUser> BusinessUsers { get; set; }

        public async Task<Business?> GetBusiness(Guid businessGuid)
        {
            var result = await Businesses.Where(x => x.Guid == businessGuid).Include(b => b.Services).ThenInclude(s => s.Repeats).SingleOrDefaultAsync();
            if(result == null)
            {
                // TODO logging
                return null;
            }

            return result;
        }

        public async Task<Business?> GetBusinessForUser(string userId)
        {
            var result =  await BusinessUsers.Include(u => u.Business).SingleOrDefaultAsync(x => x.UserId == userId);

            if(result == null)
            {
                // TODO logging
                return null;
            }

            return result.Business;
        }

        public async Task<bool> RegisterBusiness(string userId, Business business)
        {
            using var transaction = Database.BeginTransaction();

            // We should never try and register a business if the user already has a business.
            if (BusinessUsers.Any(x => x.UserId == userId))
            {
                throw new InvalidOperationException("Existing business found");
            }

            Businesses.Add(business);
            await SaveChangesAsync();

            var businessUser = new BusinessUser()
            {
                UserId = userId,
                BusinessId = business.Id,
            };

            BusinessUsers.Add(businessUser);
            await SaveChangesAsync();

            transaction.Commit();

            return true;
        }
    }
}
