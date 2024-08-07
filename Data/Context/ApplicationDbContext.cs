using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    internal partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<IdentityUser>(options)
    {
        private const string connectionString = "Server=(localdb)\\mssqllocaldb;Database=BookingSolutions;Trusted_Connection=True;";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(connectionString,
                    builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

            base.OnConfiguring(options);
        }
    }
}
