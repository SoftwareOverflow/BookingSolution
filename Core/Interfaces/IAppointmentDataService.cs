using Core.Dto;

namespace Core.Interfaces
{
    public interface IAppointmentDataService
    {
        public Task<ServiceResult<List<Appointment>>> GetBookingsBetweenDates(DateOnly start, DateOnly end);

        public Task<ServiceResult<Appointment>> GetErrors();
    }
}
