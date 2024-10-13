using Core.Dto.Appointment;
using Core.Responses;

namespace Core.Interfaces
{
    public interface IAppointmentService
    {
        public Task<ServiceResult<List<AppointmentDtoBase>>> GetAppointmentsBetweenDates(DateOnly start, DateOnly end);

        public Task<ServiceResult<AppointmentDto>> CreateOrUpdateAppointment(AppointmentDto appointment);

        public Task<ServiceResult<bool>> DeleteAppointment(Guid id);
    }
}
