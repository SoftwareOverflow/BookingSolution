using Core.Dto;

namespace Core.Interfaces
{
    public interface IEventBookingDataService
    {

        public Task<List<EventBooking>> GetBookingsBetweenDates(DateOnly start, DateOnly end);

    }
}
