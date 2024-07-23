using Core.Dto;
using Core.Interfaces;

namespace Core.Services
{
    public class EventBookingService : IEventBookingDataService
    {
        public Task<List<EventBooking>> GetBookingsBetweenDates(DateOnly start, DateOnly end)
        {
            DateTime date = new DateTime(2024, 7, 15, 12, 0, 0);

            return Task.FromResult(new List<EventBooking>
            {
                new() {
                    Name = "Event #1",
                    StartTime = new DateTime(DateOnly.FromDateTime(date), new TimeOnly(1, 30)),
                    EndTime = new DateTime(DateOnly.FromDateTime(date), new TimeOnly(2, 30)),
                    EventPaddingEnd = TimeSpan.FromMinutes(15),
                    EventPaddingStart = TimeSpan.FromMinutes(15),
                    Location = "Home"
                },

                new() {
                    Name = "Event #2 - Clash",
                    StartTime = new DateTime(DateOnly.FromDateTime(date), new TimeOnly(2, 30)),
                    EndTime = new DateTime(DateOnly.FromDateTime(date), new TimeOnly(3, 30)),
                    EventPaddingEnd = TimeSpan.FromMinutes(0),
                    EventPaddingStart = TimeSpan.FromMinutes(0),
                    Location = "Home"
                },

                new() {
                    Name = "Event #3 - Breakfast",
                    StartTime = new DateTime(DateOnly.FromDateTime(date), new TimeOnly(5, 30)),
                    EndTime = new DateTime(DateOnly.FromDateTime(date), new TimeOnly(9, 0)),
                    EventPaddingEnd = TimeSpan.FromMinutes(20),
                    EventPaddingStart = TimeSpan.FromMinutes(18),
                    Location = "Studio"
                }
            }.ToList());
        }
    }
}
