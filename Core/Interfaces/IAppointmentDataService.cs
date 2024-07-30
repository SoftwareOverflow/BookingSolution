using Core.Dto;

namespace Core.Interfaces
{
    public interface IAppointmentDataService
    {
        public Task<List<Appointment>> GetBookingsBetweenDates(DateOnly start, DateOnly end);

    }
}
