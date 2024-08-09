using AutoMapper;
using Core.Dto;
using Core.Interfaces;
using Data.Entity;
using Data.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Core.Services
{
    internal class BusinessService : IBusinessService
    {
        private readonly IBusinessContext Context;
        private readonly IMapper Mapper;
        public BusinessService(IBusinessContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }
        public async Task<IdentityResult> RegisterBusiness(Func<Task<ApplicationUser?>> registerUser, BusinessDto dto)
        {
            // TODO create the user, then create the business, then update the user with the business Id.
            // TODO we'd rather have orphaned people than businesses I think...
            try
            {
                var entity = Mapper.Map<Business>(dto);
                var result = await Context.RegisterBusiness(registerUser, entity);

                if (result)
                {
                    return IdentityResult.Success;
                }
            }
            catch (Exception ex)
            {
                // TODO log properly

            }

            return IdentityResult.Failed(new IdentityError { Description = "Internal Server Error" });
        }
    }
}
