using Core.Dto;
using Core.Responses;

namespace Core.Interfaces
{
    public interface IAppointmentDataService
    {
        public Task<ServiceResult<List<Appointment>>> GetBookingsBetweenDates(DateOnly start, DateOnly end);

        public Task<ServiceResult<Appointment>> GetErrors();
    }
}
