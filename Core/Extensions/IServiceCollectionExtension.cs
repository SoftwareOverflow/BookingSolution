using Core.Interfaces;
using Core.Services;
using Data.Extensions;
using LocalNetCoreAuth.IndividualAccounts.Components.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddApplicationLayers(this IServiceCollection services)
        {
            services.AddAuthenticationLayer();
            services.AddPersistanceLayer();

            services.AddTransient<IAppointmentDataService, AppointmentService>();
        }
    }
}
