using AutoMapper;
using Core.Dto;
using Data.Entity;

namespace Core.Mapping
{
    internal class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<BusinessDto, Business>();
            CreateMap<AddressDto, Address>();
        }
    }
}
