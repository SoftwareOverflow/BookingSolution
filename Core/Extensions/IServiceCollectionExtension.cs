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
        public static void AddApplicationLayers(this IServiceCollection services, ApplicationLayers layers)
        {
            if (layers == ApplicationLayers.AdminConsole)
            {
                services.AddAuthenticationLayer();
                services.AddAdminConsolePersistance();

                services.AddAutoMapper(typeof(AutoMapperConfig));

                services.AddTransient<IAppointmentService, AppointmentService>();
                services.AddTransient<ITimeBlockService, TimeBlockService>();
                services.AddTransient<IServiceTypeService, ServiceTypeService>();
                services.AddTransient<IBusinessService, BusinessService>();
                services.AddTransient<ISetupService, SetupService>();

                services.AddScoped<IMessageService, MessageService>();

                // Create UserService for both internal and external use
                services.AddScoped<IUserServiceInternal, UserService>();
                services.AddScoped<IUserService>(x => x.GetRequiredService<IUserServiceInternal>());
            }
            else if (layers == ApplicationLayers.BookingService)
            {
                services.AddBookingServicePersistance();

                services.AddAutoMapper(typeof(AutoMapperConfig));

                services.AddTransient<IBookingService, BookingService>();

                services.AddScoped<IMessageService, MessageService>();
            }
        }
    }
}
