using Auth.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Context
{
    internal class AuthenticationContext(DbContextOptions<AuthenticationContext> options) : IdentityDbContext<ApplicationUser>(options)
    {

        private const string connectionString = "Server=(localdb)\\mssqllocaldb;Database=BookingSolutionsAuth;Trusted_Connection=True;";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(connectionString,
                    builder => builder.MigrationsAssembly(typeof(AuthenticationContext).Assembly.FullName));

            base.OnConfiguring(options);
        }
    }
}
