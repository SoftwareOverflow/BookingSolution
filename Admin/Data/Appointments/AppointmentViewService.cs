using Admin.Data.Helpers;
using Core.Dto;
using Core.Interfaces;

namespace Admin.Data.Events
{
    public class AppointmentViewService(IAppointmentService dataService, IMessageService messages) : ViewServiceBase(messages)
    {
        public async Task<List<PositionedAppointment>> GetEvents(DateOnly start, DateOnly end)
        {
            // TODO error handling in more generic ways so that errors are always shown
            var result = await base.HandleServiceRequest<List<AppointmentDto>>(() =>
                dataService.GetAppointmentsBetweenDates(start, end)
                );

            if(result == null)
            {
                return [];
            } else
            {
                return result.GetPositionedEventBookings();
            }
        }
    }
}