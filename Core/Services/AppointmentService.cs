using Core.Dto;
using Core.Interfaces;
using Core.Responses;

namespace Core.Services
{
    internal class AppointmentService : IAppointmentService
    {
        /// <summary>
        /// Get all <see cref="AppointmentDto"/> objects in a given date range.
        /// </summary>
        /// <param name="start">The start date of the range (inclusive)</param>
        /// <param name="end">The end date of the range (inclusive)</param>
        /// <returns>Any events which occur in whole or part within the specified range</returns>
        public async Task<ServiceResult<List<AppointmentDto>>> GetAppointmentsBetweenDates(DateOnly start, DateOnly end)
        {
            var day1 = DateOnly.FromDateTime(DateTime.Today);
            var events = new List<AppointmentDto>
            {
                //region previous
                new("Very Old Event", new PersonDto())
                {
                    StartTime = new DateTime(day1.AddDays(-7), new TimeOnly(1, 0)),
                    EndTime = new DateTime(day1.AddDays(-7), new TimeOnly(2, 0)),
                },

                new("Less Old Event", new PersonDto())
                {
                    StartTime = new DateTime(day1.AddDays(-5), new TimeOnly(9, 0)),
                    EndTime = new DateTime(day1.AddDays(-5), new TimeOnly(10, 0)),
                    PaddingStart = TimeSpan.FromMinutes(15),
                    PaddingEnd = TimeSpan.FromMinutes(15)
                },

                //region day1
                new("1, Atrosciously Long Name which will have to overflow or wrap, depending on the layout required", new PersonDto())
                {
                    StartTime = new DateTime(day1, new TimeOnly(1, 0)),
                    EndTime = new DateTime(day1, new TimeOnly(2, 0)),
                },
                new("2, Pad Start", new PersonDto())
                {
                    StartTime = new DateTime(day1, new TimeOnly(1, 30)),
                    EndTime = new DateTime(day1, new TimeOnly(2, 0)),
                    PaddingStart = TimeSpan.FromMinutes(15)
                },
                new("3, Pad End", new PersonDto())
                {
                    StartTime = new DateTime(day1, new TimeOnly(1, 30)),
                    EndTime = new DateTime(day1, new TimeOnly(3, 0)),
                    PaddingEnd = TimeSpan.FromMinutes(30)
                },
                new("4, Pad Both", new PersonDto())
                {
                    StartTime = new DateTime(day1, new TimeOnly(2, 30)),
                    EndTime = new DateTime(day1, new TimeOnly(3, 30)),
                    PaddingStart = TimeSpan.FromMinutes(10),
                    PaddingEnd = TimeSpan.FromMinutes(30)
                },
                new("5", new PersonDto())
                {
                    StartTime = new DateTime(day1, new TimeOnly(4, 0)),
                    EndTime = new DateTime(day1, new TimeOnly(5, 30)),
                },
                //endregion day1

                //day2
                new("6", new PersonDto())
                {
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(9, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(12, 0)),
                },
                new("7", new PersonDto())
                {
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(13, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(14, 30)),
                },
                new ("8", new PersonDto())
                {
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(13, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(14, 30)),
                },
                new("9", new PersonDto())
                {
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(22, 0)),
                    EndTime = new DateTime(day1.AddDays(3), new TimeOnly(1, 30)),
                    PaddingStart = TimeSpan.FromMinutes(180),
                    PaddingEnd = TimeSpan.FromMinutes(100)
                },
                new("10", new PersonDto())
                {
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(23, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(23, 59, 59)),
                },

                //day 3
                new ("11", new PersonDto())
                {
                    StartTime = new DateTime(day1.AddDays(2), new TimeOnly(2, 45)),
                    EndTime = new DateTime(day1.AddDays(2), new TimeOnly(4, 50)),
                },
            };

            // TODO filter these records

            await Task.Delay(1000);

            return new ServiceResult<List<AppointmentDto>>(events);
        }

        public async Task<ServiceResult<AppointmentDto>> GetErrors()
        {
            await Task.Delay(500);

            var result = new ServiceResult<AppointmentDto>(null, ResultType.ServerError);
            result.Errors.Add("An Unkown error occurred");

            await Task.Delay(500);
            result.Errors.Add("Another error happened - YIKES!");

            return result;
        }
    }
}
