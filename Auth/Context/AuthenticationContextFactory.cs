using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
