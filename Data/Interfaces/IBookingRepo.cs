using Data.Entity;
using Data.Entity.Appointments;

namespace Data.Interfaces
{
    public interface IBookingRepo
    {
        public Task<ICollection<Service>> GetServicesForBusiness(Guid BusinessGuid);

        public Task<Service?> GetService(Guid businessGuid, Guid serviceGuid);

        public Task<ICollection<Appointment>> GetBookingsBetweenDates(Guid businessGuid, DateOnly startDate, DateOnly endDate);

        public Task<bool> CreateBookingRequest(Appointment appointment, Guid businessGuid);

        public Task<ICollection<TimeBlock>> GetTimeBlocksForBusiness(Guid businessGuid);

        public Task<ICollection<TimeBlockException>> GetTimeBlockExceptionsBetweenDates(Guid businessGuid, DateOnly startDate, DateOnly endDate);
    }
}
