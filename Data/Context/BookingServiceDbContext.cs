using Data.Entity;
using Data.Entity.Appointments;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    internal class BookingServiceDbContext(DbContextOptions<BookingServiceDbContext> options) : DbContext(options)
    {
        public DbSet<Business> Businesses { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<Person> People { get; set; }

        public override int SaveChanges()
        {
            ContextCommon.SetGuidsOnAdd(ChangeTracker);

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ContextCommon.SetGuidsOnAdd(ChangeTracker);

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}
