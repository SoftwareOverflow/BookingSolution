using Auth.Components.Account;
using Auth.Context;
using Auth.Data;
using Auth.Interfaces;
using Auth.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddAuthenticationLayer(this IServiceCollection services)
        {
            services.AddCascadingAuthenticationState();
            services.AddScoped<IdentityUserAccessor>();
            services.AddScoped<IdentityRedirectManager>();
            services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
                .AddIdentityCookies();

            services.AddDbContext<AuthenticationContext>();

            services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false) // TODO change this to use environment settings
            .AddEntityFrameworkStores<AuthenticationContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

            services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();


            services.AddScoped<UserService>();
            services.AddScoped<IUserServiceInternal>(sp => sp.GetRequiredService<UserService>());
            services.AddScoped<IUserService>(sp => sp.GetRequiredService<UserService>());
        }

        public static void AddAuthenticationLayer(this WebApplication app)
        {
            app.MapAdditionalIdentityEndpoints();
        }
    }
}
