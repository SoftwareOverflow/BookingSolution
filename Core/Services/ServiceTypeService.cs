using Core.Dto;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Drawing;

namespace Core.Services
{
    internal class ServiceTypeService : IServiceTypeService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>The assosciated Services</returns>
        public async Task<List<ServiceType>> GetServiceTypes()
        {
            var results = new List<ServiceType>
            {
                new ServiceType
                {
                    Name = "Pedicure",
                    Description = "Just Your Basic Pedi!",
                    Location = "Studio",
                    DisplayColor = Color.DarkOliveGreen,
                    Price = 29.99m
                },

                new ServiceType
                {
                    Name = "Manicure",
                    Price = 32.99m
                },

                new ServiceType
                {
                    Name = "Arcyllic Nails (Colours)",
                    Description = "Choose from a selection of colours",
                    Price = 18.99m
                },

                new ServiceType
                {
                    Name = "Acryllic Nails (Designs)",
                    Description = "Choose from a wider selection of designs",
                    Price = 35.99m
                },

                new ServiceType
                {
                    Name = "Wedding Nails",
                    Description = "Make your nails as special as the big day! Includes a free glass of prosecco ;) Now I'm writing stuff to make the description much longer to see what happens with overlaps etc",
                    Price = 59.99m
                }
            };

            await Task.Delay(1500);

            return results;
        }
    }
}
