﻿using Core.Dto;

namespace Core.Interfaces
{
    public interface IServiceTypeService
    {
        public Task<List<ServiceType>> GetServiceTypes();
    }
}
