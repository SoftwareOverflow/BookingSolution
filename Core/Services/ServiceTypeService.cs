using Core.Dto;
using Core.Interfaces;
using Data.Interfaces;

namespace Core.Services
{
    internal class ServiceTypeService : IServiceTypeService
    {
        private IServiceContext ServiceContext;
        private UserStateManager UserStateManager;
        private List<ServiceTypeDto> ServiceTypesCache = new List<ServiceTypeDto>();

        public ServiceTypeService(IServiceContext serviceContext, UserStateManager userStateManager)
        {
            UserStateManager = userStateManager;
            ServiceContext = serviceContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The assosciated Services</returns>
        public async Task<List<ServiceTypeDto>> GetServiceTypes()
        {
            var results = new List<ServiceTypeDto>
            {
                new ServiceTypeDto
                {
                    Name = "Pedicure",
                    Description = "Just Your Basic Pedi!",
                    Price = 29.99m
                },

                new ServiceTypeDto
                {
                    Name = "Manicure",
                    Price = 32.99m
                },

                new ServiceTypeDto
                {
                    Name = "Arcyllic Nails (Colours)",
                    Description = "Choose from a selection of colours",
                    Price = 18.99m
                },

                new ServiceTypeDto
                {
                    Name = "Acryllic Nails (Designs)",
                    Description = "Choose from a wider selection of designs",
                    Price = 35.99m
                },

                new ServiceTypeDto
                {
                    Name = "Wedding Nails",
                    Description = "Make your nails as special as the big day! Includes a free glass of prosecco ;) Now I'm writing stuff to make the description much longer to see what happens with overlaps etc",
                    Price = 59.99m
                }
            };

            await Task.Delay(1500);

            // Cache this information as we have transient service
            ServiceTypesCache = results;

            return results.OrderBy(x => x.Name).ToList();
        }

        public async Task<ServiceTypeDto?> GetServiceTypeByName(string? name)
        {
            //Attempt to load form the transient service cache.
            try
            {
                var item = ServiceTypesCache.Single(x => x.Name == name);
            } catch(InvalidOperationException)
            {
                return null;
            }

            // Attempt to load from the database in case the temporary cache is not right
            await Task.Delay(100);

            //Invalid
            return null;
        }
    }
}
