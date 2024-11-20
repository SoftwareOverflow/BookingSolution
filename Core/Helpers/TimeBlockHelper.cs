using Core.Dto.Appointment;
using Core.Services;

namespace Core.Helpers
{
    internal static class TimeBlockHelper
    {
        public static ICollection<TimeBlockInstanceDto> GetTimeBlockInstancesBetweenDates(ICollection<TimeBlockDto> timeBlocks, DateOnly start, DateOnly end)
        {
            var timeBlockInstances = new List<TimeBlockInstanceDto>();

            foreach (var timeBlock in timeBlocks)
            {
                if (timeBlock.RepeatType == null)
                {
                    if (timeBlock.StartTime <= end.ToDateTime(TimeOnly.MaxValue) && timeBlock.EndTime >= start.ToDateTime(TimeOnly.MinValue))
                    {
                        timeBlockInstances.Add(new(timeBlock.Guid, timeBlock.Name, DateOnly.FromDateTime(timeBlock.StartTime), IsException: false, IsOneOff: true)
                        {
                            StartTime = timeBlock.StartTime,
                            EndTime = timeBlock.EndTime,
                        });
                    }

                    // No repeats to check, continue
                    continue;
                }

                var days = timeBlock.EndTime.Subtract(timeBlock.StartTime).Days;
                var startTime = TimeOnly.FromDateTime(timeBlock.StartTime);
                var endTime = TimeOnly.FromDateTime(timeBlock.EndTime);

                var date = start;

                if (date < DateOnly.FromDateTime(timeBlock.StartTime))
                {
                    date = DateOnly.FromDateTime(timeBlock.StartTime);
                }

                while (date <= end)
                {
                    var result = RepeaterService.GetNextRepeaterDate(timeBlock, date);
                    if (result.IsSuccess)
                    {
                        date = result.Result;

                        // Next occurance is outside range
                        if (date > end)
                        {
                            break;
                        }

                        // Handle all repeats which are NOT exceptions
                        var exception = timeBlock.Exceptions.SingleOrDefault(e => e.DateToReplace == date);
                        if (exception == null)
                        {
                            var timeBlockInstance = new TimeBlockInstanceDto(timeBlock.Guid, timeBlock.Name, date, IsException: false)
                            {
                                StartTime = new DateTime(date, startTime),
                                EndTime = new DateTime(date, endTime).AddDays(days),
                            };

                            timeBlockInstances.Add(timeBlockInstance);
                        }
                    }

                    date = date.AddDays(1);
                }
            }

            return timeBlockInstances;
        }
    }
}
