using Core.Dto;
using Data.Entity;
using Microsoft.AspNetCore.Identity;

namespace Core.Interfaces
{
    public interface IBusinessService
    {
        public Task<IdentityResult> RegisterBusiness(string userId, BusinessDto dto);
    }
}
