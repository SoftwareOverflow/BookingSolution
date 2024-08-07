using Data.Entity;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    internal partial class ApplicationDbContext : IServiceContext
    {
        public DbSet<Service> Services { get; set; }

        // TODO add some sort of BusinessOwndedEntity which contains the FK to Business.
        // Add global filter for that (see TradeTrack example)
        public async Task<IEnumerable<Service>> GetAllServices()
        {
            return await Services.ToListAsync();
        }
    }
}
