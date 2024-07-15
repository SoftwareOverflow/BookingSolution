using Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddPersistanceLayer(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>();

            services.AddIdentityCore<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false) // TODO change this in appSettings for debug, dev, int and prod.
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

        }
    }
}
