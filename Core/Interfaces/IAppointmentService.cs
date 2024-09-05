using Core.Dto;
using Core.Responses;

namespace Core.Interfaces
{
    public interface IAppointmentService
    {
        public Task<ServiceResult<List<AppointmentDto>>> GetAppointmentsBetweenDates(DateOnly start, DateOnly end);

        public Task<ServiceResult<AppointmentDto>> GetErrors();
    }
}
