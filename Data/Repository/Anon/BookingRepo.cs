using Data.Context;
using Data.Entity;
using Data.Entity.Appointments;
using Data.Extensions;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository.Anon
{
    internal class BookingRepo(IDbContextFactory<BookingServiceDbContext> factory) : BaseAnonRepo<BookingServiceDbContext>(factory), IBookingRepo
    {
        public async Task<ICollection<Service>> GetServicesForBusiness(Guid businessGuid)
        {
            return await ExecuteAsync(async (db) =>
            {
                var business = await db.Businesses.Include(b => b.Services).SingleOrDefaultAsync(b => b.Guid == businessGuid) ?? throw new ArgumentException("Cannot find business");

                return business?.Services;
            }) ?? [];
        }

        public async Task<Service?> GetService(Guid businessGuid, Guid serviceGuid)
        {
            return await ExecuteAsync(async (db) =>
            {
                var business = await db.Businesses.Where(x => x.Guid == businessGuid).Include(x => x.Services).ThenInclude(s => s.Repeats).SingleOrDefaultAsync();
                return business?.Services.SingleOrDefault(s => s.Guid == serviceGuid);
            });
        }

        public async Task<ICollection<TimeBlock>> GetTimeBlocksForBusiness(Guid businessGuid)
        {
            return await ExecuteAsync(async (db) =>
            {
                var business = await db.Businesses.SingleOrDefaultAsync(b => b.Guid == businessGuid) ?? throw new ArgumentException("Cannot find business");
                return db.TimeBlocks.Include(tb => tb.Repeats).Include(tb => tb.Exceptions).Where(tb => tb.BusinessId == business.Id).ToList();
            }) ?? [];
        }

        public async Task<ICollection<TimeBlockException>> GetTimeBlockExceptionsBetweenDates(Guid businessGuid, DateOnly startDate, DateOnly endDate)
        {
            return await ExecuteAsync(async (db) =>
            {
                var business = await db.Businesses.SingleOrDefaultAsync(b => b.Guid == businessGuid) ?? throw new ArgumentException("Cannot find business");
                return db.TimeBlockExceptions.Where(tb => tb.BusinessId == business.Id).AsQueryable().BetweenDates(startDate, endDate).ToList();
            }) ?? [];
        }

        public async Task<ICollection<Appointment>> GetBookingsBetweenDates(Guid businessGuid, DateOnly startDate, DateOnly endDate)
        {
            return await ExecuteAsync(async (db) =>
            {
                var business = await db.Businesses.SingleOrDefaultAsync(x => x.Guid == businessGuid);
                var businessId = business?.Id ?? throw new ArgumentException("Cannot find business");

                return db.Appointments.Where(a => a.BusinessId == businessId).BetweenDates(startDate, endDate).ToList();
            }) ?? [];
        }

        public async Task<bool> CreateBookingRequest(Appointment appointment, Guid businessGuid)
        {
            if (appointment.Person == null)
            {
                throw new ArgumentException("Unable create appointment - Person details required");
            }

            await ExecuteVoidAsync(async (db) =>
            {
                var business = await db.Businesses.Where(x => x.Guid == businessGuid).Include(b => b.Services).SingleOrDefaultAsync();
                var service = business?.Services.SingleOrDefault(s => s.Guid == appointment.Service!.Guid);

                if (business == null || service == null)
                {
                    throw new ArgumentException("Unable to create booking request - Cannot find business and service");
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
