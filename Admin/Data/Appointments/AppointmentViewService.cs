using Core.Dto;
using Core.Interfaces;

namespace Admin.Data.Events
{
    public class AppointmentViewService(IAppointmentDataService dataService)
    {
        public async Task<List<PositionedAppointment>> GetEvents(DateOnly start, DateOnly end)
        {
            // TODO error handling
            var events = await dataService.GetBookingsBetweenDates(start, end);

            return events.GetPositionedEventBookings();
        }
    }
}