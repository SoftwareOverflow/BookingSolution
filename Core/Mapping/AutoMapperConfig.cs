using AutoMapper;
using Core.Dto;
using Data.Entity;

namespace Core.Mapping
{
    internal class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<BusinessDto, Business>().ReverseMap();
            CreateMap<AddressDto, Address>().ReverseMap();

            CreateMap<ServiceTypeDto, Service>().ReverseMap();
        }
    }
}
