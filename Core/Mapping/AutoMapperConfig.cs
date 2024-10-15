using AutoMapper;
using Core.Dto;
using Core.Dto.Appointment;
using Data.Entity;
using Data.Entity.Appointments;

namespace Core.Mapping
{
    internal class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            //Mapping custom types
            CreateMap<DateTime, DateOnly>().ConvertUsing(dt => DateOnly.FromDateTime(dt));
            CreateMap<DateOnly, DateTime>().ConvertUsing(d => new DateTime(d, new TimeOnly()));
            CreateMap<TimeOnly, TimeSpan>().ConvertUsing(t => t.ToTimeSpan());
            CreateMap<TimeSpan, TimeOnly>().ConvertUsing(ts => TimeOnly.FromTimeSpan(ts));

            CreateMap<BusinessDto, Business>().ReverseMap();
            CreateMap<AddressDto, Address>().ReverseMap();

            CreateMap<ServiceTypeDto, Service>().ReverseMap();
            CreateMap<RepeaterDto, ServiceRepeater>().ReverseMap();

            CreateMap<AppointmentDtoBase, BaseAppointment>().Include<AppointmentDto, Appointment>().ReverseMap();

            CreateMap<AppointmentDto, Appointment>().ReverseMap();
            CreateMap<PersonDto, Person>().ReverseMap();

            CreateMap<TimeBlockDto, TimeBlock>().ReverseMap();
            CreateMap<TimeBlockExceptionDto, TimeBlockException>().ReverseMap();
            CreateMap<RepeaterDto, TimeBlockRepeater>().ReverseMap();
        }
    }
}
