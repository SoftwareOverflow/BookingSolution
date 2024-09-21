using Data.Context;
using Data.Entity;
using Data.Entity.Appointments;
using Data.Extensions;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository
{
    internal class BookingRepo(IDbContextFactory<ApplicationDbContext> factory) : BaseRepo(factory), IBookingRepo
    {
        public async Task<Service?> GetService(Guid businessGuid, Guid serviceGuid)
        {
            return await ExecuteAsync(async (db) =>
            {
                var business = await db.Businesses.Where(x => x.Guid == businessGuid).Include(x => x.Services).ThenInclude(s => s.Repeats).IgnoreQueryFilters().SingleOrDefaultAsync();
                return business?.Services.SingleOrDefault(s => s.Guid == serviceGuid);
            });
        }

        public async Task<ICollection<Appointment>> GetBookingsBetweenDates(Guid businessGuid, DateOnly startDate, DateOnly endDate)
        {
            return await ExecuteAsync(async (db) =>
            {
                var business = await db.Businesses.SingleOrDefaultAsync(x => x.Guid == businessGuid);
                var businessId = business?.Id ?? throw new ArgumentException("Cannot find business");

                // Ignore the filter queries because we won't be logged in at this point, but we still need to match to the business
                return db.Appointments.IgnoreQueryFilters().BetweenDates(startDate, endDate).ToList();
            }) ?? [];
        }

        public async Task<bool> CreateBookingRequest(Appointment appointment, Guid businessGuid)
        {
            await ExecuteVoidAsync(async (db) =>
            {
                //Ignore filter queries - not logged in here but need to match business and service.
                var business = await db.Businesses.Where(x => x.Guid == businessGuid).Include(b => b.Services).IgnoreQueryFilters().SingleOrDefaultAsync();
                var service = business?.Services.SingleOrDefault(s => s.Guid == appointment.Service!.Guid);

                if (business == null || service == null)
                {
                    throw new ArgumentException("Unable to create booking request - unable to find business and service");
                }

                appointment.BusinessId = business.Id;
                appointment.Service = service; // Ensure the service mathces (including id) otherwise EF will try and create a new service and break FK constraint to business

                if (appointment.Person == null)
                {
                    throw new ArgumentException("Unable to create booking request - person required");
                }

                db.Appointments.Add(appointment);
                await db.SaveChangesAsync();
            });

            return true;
        }
    }
}
