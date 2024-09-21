using Data.Context;
using Data.Entity;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Data.Repository
{
    internal class ServiceRepo(IDbContextFactory<ApplicationDbContext> factory) : BaseRepo(factory), IServiceRepo
    {
        public async Task<bool> Create(string userId, Service service)
        {
            await ExecuteVoidAsync(async (db) =>
            {
                var existing = await db.Services.SingleOrDefaultAsync(x => x.Guid == service.Guid);

                if (existing != null)
                {
                    throw new DuplicateNameException($"Id {service.Guid} already exists in the database. Call Update to update this record");
                }

                var businessId = (await db.BusinessUsers.SingleOrDefaultAsync(x => x.UserId == userId))?.BusinessId
                    ?? throw new ArgumentException($"Unable to locate business for user with id {userId}");

                service.BusinessId = businessId;

                db.Services.Add(service);
                await db.SaveChangesAsync();
            });

            return true;
        }

        public async Task<bool> Update(string userId, Service service)
        {
            await ExecuteVoidAsync(async (db) =>
            {
                var existingService = await db.Services.Include(s => s.Repeats).SingleOrDefaultAsync(x => x.Guid == service.Guid);

                if (existingService != null && service.Guid != Guid.Empty)
                {
                    var businessId = (await db.BusinessUsers.SingleOrDefaultAsync(x => x.UserId == userId))?.BusinessId
                    ?? throw new ArgumentException($"Unable to locate business for user with id {userId}");

                    if (service.BusinessId != businessId)
                    {
                        throw new InvalidOperationException("Unable to match the business id with the provided UserId");
                    }

                    service.Id = existingService.Id;

                    // The nested ServiceRepeater objects do NOT get updated by default for some reason. Manually force copy them
                    existingService.Repeats = service.Repeats;

                    db.Services.Entry(existingService).CurrentValues.SetValues(service);
                    db.Services.Update(existingService);

                    await db.SaveChangesAsync();
                }
                else
                {
                    // TODO logging

                    throw new ArgumentException($"Unable to update service - cannot find id {service.Guid} in the database");
                }
            });

            return true;
        }

        // TODO add some sort of BusinessOwndedEntity which contains the FK to Business.
        // Add global filter for that (see TradeTrack example)
        public async Task<IEnumerable<Service>> GetAllServicesForUser()
        {
            return await ExecuteAsync(async (db) =>
            {
                return await db.Services.Include(s => s.Repeats).ToListAsync();
            }) ?? [];
        }

        public async Task<bool> Delete(Guid serviceId)
        {
            await ExecuteVoidAsync(async (db) =>
            {
                var toRemove = await db.Services.SingleOrDefaultAsync(x => x.Guid == serviceId);
                if (toRemove != null)
                {
                    db.Services.Remove(toRemove);
                    await db.SaveChangesAsync();
                }
                else
                {
                    // TODO logging
                    throw new ArgumentException("Unable to find service with the provided id");
                }
            });

            return true;
        }
    }
}
