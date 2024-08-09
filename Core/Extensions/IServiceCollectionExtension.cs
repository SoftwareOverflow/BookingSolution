using AutoMapper;
using Core.Dto;
using Core.Interfaces;
using Core.Mapping;
using Core.Services;
using Data.Entity;
using Data.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddApplicationLayers(this IServiceCollection services)
        {
            services.AddPersistanceLayer();

            services.AddAutoMapper(typeof(AutoMapperConfig));

            services.AddTransient<IAppointmentDataService, AppointmentService>();
            services.AddTransient<IServiceTypeService, ServiceTypeService>();
            services.AddTransient<IBusinessService, BusinessService>();
        }
    }
}
