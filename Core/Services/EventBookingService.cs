using Core.Dto;
using Core.Interfaces;

namespace Core.Services
{
    internal class EventBookingService : IEventBookingDataService
    {
        /// <summary>
        /// Get all <see cref="EventBooking"/> objects in a given date range.
        /// </summary>
        /// <param name="start">The start date of the range (inclusive)</param>
        /// <param name="end">The end date of the range (inclusive)</param>
        /// <returns>Any events which occur in whole or part within the specified range</returns>
        public Task<List<EventBooking>> GetBookingsBetweenDates(DateOnly start, DateOnly end)
        {


            var day1 = DateOnly.FromDateTime(DateTime.Today);
            var events = new List<EventBooking>
            {
                //region day1
                new EventBooking
                {
                    Name = "1, Atrosciously Long Name",
                    Location = "Studio",
                    StartTime = new DateTime(day1, new TimeOnly(1, 0)),
                    EndTime = new DateTime(day1, new TimeOnly(2, 0)),
                },
                new EventBooking
                {
                    Name = "2, Pad Start",
                    StartTime = new DateTime(day1, new TimeOnly(1, 30)),
                    EndTime = new DateTime(day1, new TimeOnly(2, 0)),
                    PaddingStart = TimeSpan.FromMinutes(15)
                },
                new EventBooking
                {
                    Name = "3, Pad End",
                    StartTime = new DateTime(day1, new TimeOnly(1, 30)),
                    EndTime = new DateTime(day1, new TimeOnly(3, 0)),
                    PaddingEnd = TimeSpan.FromMinutes(30)
                },
                new EventBooking
                {
                    Name = "4, Pad Both",
                    StartTime = new DateTime(day1, new TimeOnly(2, 30)),
                    EndTime = new DateTime(day1, new TimeOnly(3, 30)),
                    PaddingStart = TimeSpan.FromMinutes(10),
                    PaddingEnd = TimeSpan.FromMinutes(30)
                },
                new EventBooking
                {
                    Name = "5",
                    StartTime = new DateTime(day1, new TimeOnly(4, 0)),
                    EndTime = new DateTime(day1, new TimeOnly(5, 30)),
                },
                //endregion day1

                //day2
                new EventBooking
                {
                    Name = "6",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(9, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(12, 0)),
                },
                new EventBooking
                {
                    Name = "7",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(13, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(14, 30)),
                },
                new EventBooking
                {
                    Name = "8",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(13, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(14, 30)),
                },
                new EventBooking
                {
                    Name = "9",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(22, 0)),
                    EndTime = new DateTime(day1.AddDays(3), new TimeOnly(1, 30)),
                    PaddingStart = TimeSpan.FromMinutes(180),
                    PaddingEnd = TimeSpan.FromMinutes(100)
                },
                new EventBooking
                {
                    Name = "10",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(23, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(23, 59, 59)),
                },

                //day 3
                new EventBooking
                {
                    Name = "11",
                    StartTime = new DateTime(day1.AddDays(2), new TimeOnly(2, 45)),
                    EndTime = new DateTime(day1.AddDays(2), new TimeOnly(4, 50)),
                },
            };

            // TODO filter these records

            return Task.FromResult(events);
        }

        private DateTime PaddedStart(EventBooking e) => e.StartTime.Subtract(e.PaddingStart); 
        private DateTime PaddedEnd(EventBooking e) => e.EndTime.Add(e.PaddingEnd); 
    }
}
