using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Auth.Context
{
    internal class AuthenticationContextFactory : IDesignTimeDbContextFactory<AuthenticationContext>
    {
        private const string connectionString = "Server=(localdb)\\mssqllocaldb;Database=BookingSolutionsAuth;Trusted_Connection=True;";

        public AuthenticationContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AuthenticationContext>();
            optionsBuilder.UseSqlServer(connectionString,
                builder => builder.MigrationsAssembly(typeof(AuthenticationContext).Assembly.FullName));

            return new AuthenticationContext(optionsBuilder.Options);
        }
    }
}
