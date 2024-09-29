using Data.Context;
using Data.Entity;
using Data.Interfaces;
using Data.Repository;
using Data.Repository.Anon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Extensions
{
    public static class IServiceCollectionExtensions
    {
        // TODO remove connectionString from here. Load from settings file.
        private const string connectionString = "Server=(localdb)\\mssqllocaldb;Database=BookingSolutions;Trusted_Connection=True;";

        public static void AddAdminConsolePersistance(this IServiceCollection services)
        {
            services.AddDbContextFactory<ApplicationDbContext>(
                                options =>
                                {
                                    options.UseSqlServer(connectionString, builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
#if DEBUG
                                    options.EnableSensitiveDataLogging();
#endif
                                },
                                ServiceLifetime.Scoped
                );


            services.AddTransient<IServiceRepo, ServiceRepo>();
            services.AddTransient<IBusinessRepo, BusinessRepo>();
            services.AddTransient<IAppointmentRepo, AppointmentRepo>();
        }

        public static void AddBookingServicePersistance(this IServiceCollection services)
        {
            services.AddDbContextFactory<BookingServiceDbContext>(
                                options =>
                                {
                                    options.UseSqlServer(connectionString, builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
#if DEBUG
                                    options.EnableSensitiveDataLogging();
#endif
                                },
                                ServiceLifetime.Scoped
            );

            services.AddTransient<IBookingRepo, BookingRepo>();
        }
    }
}
