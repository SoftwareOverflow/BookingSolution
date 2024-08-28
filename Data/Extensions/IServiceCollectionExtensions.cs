using Data.Context;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Extensions
{
    public static class IServiceCollectionExtensions
    {
        // TODO remove connectionString from here. Load from settings file.
        private const string connectionString = "Server=(localdb)\\mssqllocaldb;Database=BookingSolutions;Trusted_Connection=True;";

        public static void AddPersistanceLayer(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(connectionString, builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                );

            services.AddTransient<IServiceContext, ApplicationDbContext>();
            services.AddTransient<IBusinessContext, ApplicationDbContext>();
        }
    }
}
