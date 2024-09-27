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
            return await ExecuteAnonymousAsync(async (db) =>
            {
                var business = await db.Businesses.IgnoreQueryFilters().Where(x => x.Guid == businessGuid).Include(x => x.Services).ThenInclude(s => s.Repeats).SingleOrDefaultAsync();
                return business?.Services.SingleOrDefault(s => s.Guid == serviceGuid);
            });
        }

        public async Task<ICollection<Appointment>> GetBookingsBetweenDates(Guid businessGuid, DateOnly startDate, DateOnly endDate)
        {
            return await ExecuteAnonymousAsync(async (db) =>
            {

                // TODO write test for this, it should NOT return as I missed the imporant IgnoreQueryFilters
                var business = await db.Businesses.SingleOrDefaultAsync(x => x.Guid == businessGuid);
                var businessId = business?.Id ?? throw new ArgumentException("Cannot find business");

                // Ignore the filter queries because we won't be logged in at this point, but we still need to match to the business
                // TODO Write a test for this - this DOES NOT currently filter businesses and SHOULD NOT work!
                return db.Appointments.IgnoreQueryFilters().BetweenDates(startDate, endDate).ToList();
            }) ?? [];
        }

        public async Task<bool> CreateBookingRequest(Appointment appointment, Guid businessGuid)
        {
            if (appointment.Person == null)
            {
                throw new ArgumentException("Unable create appointment - Person details required");
            }

            await ExecuteAnonymousVoidAsync(async (db) =>
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

                db.Appointments.Add(appointment);
                await db.SaveChangesAsync();
            });

            return true;
        }
    }
}
