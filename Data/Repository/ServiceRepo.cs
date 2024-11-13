using Data.Context;
using Data.Entity;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository
{
    internal class ServiceRepo(IDbContextFactory<ApplicationDbContext> factory) : BaseRepo(factory), IServiceRepo
    {
        public async Task<bool> Create(Service service)
        {
            await ExecuteVoidAsync(async (db, userId) =>
            {
                var existing = await db.Services.SingleOrDefaultAsync(x => x.Guid == service.Guid);

                if (existing != null)
                {
                    throw new ArgumentException($"Id {service.Guid} already exists in the database. Call Update to update this record");
                }

                var businessId = await db.GetBusinessId();
                service.BusinessId = businessId;

                db.Services.Add(service);
                await db.SaveChangesAsync();
            });

            return true;
        }

        public async Task<bool> Update(Service service)
        {
            await ExecuteVoidAsync(async (db, userId) =>
            {
                var businessId = await db.GetBusinessId();

                var existingService = await db.Services.Include(s => s.Repeats).SingleOrDefaultAsync(x => x.Guid == service.Guid);

                if (existingService != null && service.Guid != Guid.Empty)
                {

                    service.Id = existingService.Id;
                    service.BusinessId = businessId;

                    // The nested ServiceRepeater objects do NOT get updated by default. Manually copy them
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

        public async Task<IEnumerable<Service>> GetServices()
        {
            return await ExecuteAsync(async (db, _) =>
            {
                return await db.Services.Include(s => s.Repeats).ToListAsync();
            }) ?? [];
        }

        public async Task<bool> Delete(Guid serviceId)
        {
            await ExecuteVoidAsync(async (db, _) =>
            {
                var toRemove = await db.Services.Include(s => s.Appointments).SingleOrDefaultAsync(x => x.Guid == serviceId);

                if (toRemove != null)
                {
                    // Disassociate all the appointments for this service
                    foreach (var apt in toRemove.Appointments)
                    {
                        apt.ServiceId = null;
                    }

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
