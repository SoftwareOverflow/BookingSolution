using Data.Entity;
using Data.Entity.Appointments;
using Data.Extensions;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    internal partial class ApplicationDbContext : IBookingContext
    {
        public async Task<Service?> GetService(Guid businessGuid, Guid serviceGuid)
        {
            var business = await Businesses.Where(x => x.Guid == businessGuid).Include(x => x.Services).ThenInclude(s => s.Repeats).IgnoreQueryFilters().SingleOrDefaultAsync();

            return business?.Services.SingleOrDefault(s => s.Guid == serviceGuid);
        }

        public async Task<ICollection<Appointment>> GetBookingsBetweenDates(Guid businessGuid, DateOnly startDate, DateOnly endDate)
        {
            var business = await Businesses.SingleOrDefaultAsync(x => x.Guid == businessGuid);
            var businessId = business?.Id ?? throw new ArgumentException("Cannot find business");

            // Ignore the filter queries because we won't be logged in at this point, but we still need to match to the business
            return Appointments.IgnoreQueryFilters().BetweenDates(startDate, endDate).ToList();
        }

        public async Task<bool> CreateBookingRequest(Appointment appointment, Guid businessGuid)
        {
            //Ignore filter queries - not logged in here but need to match business and service.
            var business = await Businesses.Where(x => x.Guid == businessGuid).Include(b => b.Services).IgnoreQueryFilters().SingleOrDefaultAsync();
            var service = business?.Services.SingleOrDefault(s => s.Guid == appointment.Service!.Guid);

            if(business == null || service == null)
            {
                throw new ArgumentException("Unable to create booking request - unable to find business and service");
            }
            
            appointment.BusinessId = business.Id;
            appointment.Service = service; // Ensure the service mathces (including id) otherwise EF will try and create a new service and break FK constraint to business

            if(appointment.Person == null)
            {
                throw new ArgumentException("Unable to create booking request - person required");
            }

            Appointments.Add(appointment);
            await SaveChangesAsync();

            return true;
        }
    }
}
