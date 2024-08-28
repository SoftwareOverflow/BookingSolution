using Data.Entity;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Data.Context
{
    internal partial class ApplicationDbContext : IServiceContext
    {
        public DbSet<Service> Services { get; set; }

        public async Task<bool> Create(string userId, Service service)
        {
            var existing = await Services.SingleOrDefaultAsync(x => x.Guid == service.Guid);

            if (existing != null)
            {
                throw new DuplicateNameException($"Id {service.Guid} already exists in the database. Call Update to update this record");
            }

            var businessId = (await BusinessUsers.SingleOrDefaultAsync(x => x.UserId == userId))?.BusinessId 
                ?? throw new ArgumentException($"Unable to locate business for user with id {userId}");

            service.BusinessId = businessId;

            Services.Add(service);
            await SaveChangesAsync();

            return true;
        }

        public async Task<bool> Update(string userId, Service service)
        {
            var existingService = await Services.Include(s => s.Repeats).SingleOrDefaultAsync(x => x.Guid == service.Guid);

            if (existingService != null && service.Guid != Guid.Empty)
            {
                var businessId = (await BusinessUsers.SingleOrDefaultAsync(x => x.UserId == userId))?.BusinessId
                ?? throw new ArgumentException($"Unable to locate business for user with id {userId}");

                if (service.BusinessId != businessId)
                {
                    throw new InvalidOperationException("Unable to match the business id with the provided UserId");
                }

                service.Id = existingService.Id;

                // The nested ServiceRepeater objects do NOT get updated by default for some reason. Manually force copy them
                existingService.Repeats = service.Repeats;

                Services.Entry(existingService).CurrentValues.SetValues(service);
                Services.Update(existingService);

                await SaveChangesAsync();

                return true;
            }
            else
            {
                // TODO logging

                throw new ArgumentException($"Unable to update service - cannot find id {service.Guid} in the database");
            }
        }

        // TODO add some sort of BusinessOwndedEntity which contains the FK to Business.
        // Add global filter for that (see TradeTrack example)
        public async Task<IEnumerable<Service>> GetAllServicesForUser()
        {
            return await Services.Include(s => s.Repeats).ToListAsync();
        }

        public async Task<bool> Delete(Guid serviceId)
        {
            var toRemove = await Services.SingleOrDefaultAsync(x => x.Guid == serviceId);
            if (toRemove != null)
            {
                Services.Remove(toRemove);
                await SaveChangesAsync();
                return true;
            }

            // TODO logging.
            throw new ArgumentException("Unable to find service with the provided id");
        }
    }
}
