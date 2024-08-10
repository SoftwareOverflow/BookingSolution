using Data.Context;
using Data.Entity;
using Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddPersistanceLayer(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>();

            services.AddTransient<IServiceContext, ApplicationDbContext>();
            services.AddTransient<IBusinessContext, ApplicationDbContext>();
        }
    }
}
