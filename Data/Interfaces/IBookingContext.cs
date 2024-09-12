using Data.Entity;
using Data.Entity.Appointments;

namespace Data.Interfaces
{
    public interface IBookingContext
    {
        public Task<Service?> GetService(Guid businessGuid, Guid serviceGuid);

        public Task<ICollection<Appointment>> GetBookingsBetweenDates(Guid businessGuid, DateOnly startDate, DateOnly endDate);

        public Task<bool> CreateBookingRequest(Appointment appointment, Guid businessGuid);
    }
}
