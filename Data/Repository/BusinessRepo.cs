using Data.Context;
using Data.Entity;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository
{
    internal class BusinessRepo(IDbContextFactory<ApplicationDbContext> factory) : BaseRepo(factory), IBusinessRepo
    {
        public async Task<Business?> GetBusiness(Guid businessGuid)
        {
            return await ExecuteAsync(async (db) =>
            {
                var result = await db.Businesses.Where(x => x.Guid == businessGuid).Include(b => b.Services).ThenInclude(s => s.Repeats).SingleOrDefaultAsync();
                if (result == null)
                {
                    // TODO logging
                    return null;
                }

                return result;
            });
        }

        public async Task<Business?> GetBusinessForUser(string userId)
        {
            return await ExecuteAsync(async (db) =>
            {
                var result = await db.BusinessUsers.Include(u => u.Business).SingleOrDefaultAsync(x => x.UserId == userId);

                if (result == null)
                {
                    // TODO logging
                    return null;
                }

                return result.Business;
            });
        }

        public async Task<bool> RegisterBusiness(string userId, Business business)
        {
            await ExecuteVoidAsync(async (db) =>
            {
                using var transaction = db.Database.BeginTransaction();

                // We should never try and register a business if the user already has a business.
                if (db.BusinessUsers.Any(x => x.UserId == userId))
                {
                    throw new InvalidOperationException("Existing business found");
                }

                db.Businesses.Add(business);
                await db.SaveChangesAsync();

                var businessUser = new BusinessUser()
                {
                    UserId = userId,
                    BusinessId = business.Id,
                };

                db.BusinessUsers.Add(businessUser);
                await db.SaveChangesAsync();

                transaction.Commit();
            });

            return true;
        }
    }
}
