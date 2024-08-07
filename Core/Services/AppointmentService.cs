using Core.Dto;
using Core.Interfaces;

namespace Core.Services
{
    internal class AppointmentService : IAppointmentDataService
    {
        /// <summary>
        /// Get all <see cref="Appointment"/> objects in a given date range.
        /// </summary>
        /// <param name="start">The start date of the range (inclusive)</param>
        /// <param name="end">The end date of the range (inclusive)</param>
        /// <returns>Any events which occur in whole or part within the specified range</returns>
        public async Task<ServiceResult<List<Appointment>>> GetBookingsBetweenDates(DateOnly start, DateOnly end)
        {
            var day1 = DateOnly.FromDateTime(DateTime.Today);
            var events = new List<Appointment>
            {
                //region previous
                new Appointment
                {
                    Name = "Very Old Event",
                    StartTime = new DateTime(day1.AddDays(-7), new TimeOnly(1, 0)),
                    EndTime = new DateTime(day1.AddDays(-7), new TimeOnly(2, 0)),
                },

                new Appointment
                {
                    Name = "Less Old Event",
                    StartTime = new DateTime(day1.AddDays(-5), new TimeOnly(9, 0)),
                    EndTime = new DateTime(day1.AddDays(-5), new TimeOnly(10, 0)),
                    PaddingStart = TimeSpan.FromMinutes(15),
                    PaddingEnd = TimeSpan.FromMinutes(15)
                },

                //region day1
                new Appointment
                {
                    Name = "1, Atrosciously Long Name which will have to overflow or wrap, depending on the layout required",
                    StartTime = new DateTime(day1, new TimeOnly(1, 0)),
                    EndTime = new DateTime(day1, new TimeOnly(2, 0)),
                },
                new Appointment
                {
                    Name = "2, Pad Start",
                    StartTime = new DateTime(day1, new TimeOnly(1, 30)),
                    EndTime = new DateTime(day1, new TimeOnly(2, 0)),
                    PaddingStart = TimeSpan.FromMinutes(15)
                },
                new Appointment
                {
                    Name = "3, Pad End",
                    StartTime = new DateTime(day1, new TimeOnly(1, 30)),
                    EndTime = new DateTime(day1, new TimeOnly(3, 0)),
                    PaddingEnd = TimeSpan.FromMinutes(30)
                },
                new Appointment
                {
                    Name = "4, Pad Both",
                    StartTime = new DateTime(day1, new TimeOnly(2, 30)),
                    EndTime = new DateTime(day1, new TimeOnly(3, 30)),
                    PaddingStart = TimeSpan.FromMinutes(10),
                    PaddingEnd = TimeSpan.FromMinutes(30)
                },
                new Appointment
                {
                    Name = "5",
                    StartTime = new DateTime(day1, new TimeOnly(4, 0)),
                    EndTime = new DateTime(day1, new TimeOnly(5, 30)),
                },
                //endregion day1

                //day2
                new Appointment
                {
                    Name = "6",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(9, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(12, 0)),
                },
                new Appointment
                {
                    Name = "7",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(13, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(14, 30)),
                },
                new Appointment
                {
                    Name = "8",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(13, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(14, 30)),
                },
                new Appointment
                {
                    Name = "9",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(22, 0)),
                    EndTime = new DateTime(day1.AddDays(3), new TimeOnly(1, 30)),
                    PaddingStart = TimeSpan.FromMinutes(180),
                    PaddingEnd = TimeSpan.FromMinutes(100)
                },
                new Appointment
                {
                    Name = "10",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(23, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(23, 59, 59)),
                },

                //day 3
                new Appointment
                {
                    Name = "11",
                    StartTime = new DateTime(day1.AddDays(2), new TimeOnly(2, 45)),
                    EndTime = new DateTime(day1.AddDays(2), new TimeOnly(4, 50)),
                },
            };

            // TODO filter these records

            await Task.Delay(1000);

            return new ServiceResult<List<Appointment>>(events);
        }

        public async Task<ServiceResult<Appointment>> GetErrors()
        {
            await Task.Delay(1200);

            var result = new ServiceResult<Appointment>(null, ResultType.ServerError);
            result.Errors.Add("An Unkown error occurred");
            result.Errors.Add("Another error happened - YIKES!");

            return result;
        }

        private DateTime PaddedStart(Appointment e) => e.StartTime.Subtract(e.PaddingStart); 
        private DateTime PaddedEnd(Appointment e) => e.EndTime.Add(e.PaddingEnd); 
    }
}
