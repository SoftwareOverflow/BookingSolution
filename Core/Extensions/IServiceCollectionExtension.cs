using Auth.Extensions;
using Core.Interfaces;
using Core.Mapping;
using Core.Services;
using Data.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddApplicationLayers(this IServiceCollection services)
        {
            services.AddPersistanceLayer();
            services.AddAuthenticationLayer();

            services.AddAutoMapper(typeof(AutoMapperConfig));
            
            services.AddTransient<IAppointmentDataService, AppointmentService>();
            services.AddTransient<IServiceTypeService, ServiceTypeService>();
            services.AddTransient<IBusinessService, BusinessService>();

            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<UserStateManager>();
            services.AddScoped<IUserService, UserService>();
        }
    }
}
