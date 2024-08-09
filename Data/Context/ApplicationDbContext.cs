using Data.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace Data.Context
{
    internal partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        private const string connectionString = "Server=(localdb)\\mssqllocaldb;Database=BookingSolutions;Trusted_Connection=True;";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(connectionString,
                    builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

            base.OnConfiguring(options);
        }

        public override int SaveChanges()
        {
            SetGuidsOnAdd();

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            SetGuidsOnAdd();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void SetGuidsOnAdd()
        {
            ChangeTracker.Entries().Where(x => x.Entity is BaseEntity && x.State == EntityState.Added)
                .Select(x => (BaseEntity)x.Entity).ToList()
                .ForEach(item =>
                    item.Guid = Guid.NewGuid()
                );
        }
    }
}
