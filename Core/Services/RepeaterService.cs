using Core.Dto;
using Core.Responses;

namespace Core.Services
{
    internal static class RepeaterService
    {
        public static ServiceResult<DateOnly> GetNextRepeaterDate(IRepeatable dto, DateOnly startDate)
        {
            if (dto.RepeatType != null && dto.Repeats.Count == 0)
            {
                return new ServiceResult<DateOnly>(default, ResultType.ServerError, ["Unable to find any dates for this service"]);
            }

            var currentDate = startDate;
            if (dto.RepeatType == RepeaterTypeDto.Weekly)
            {
                return GetNextWeeklyDate(dto, currentDate);
            }
            else if (dto.RepeatType == RepeaterTypeDto.MonthlyAbsolute)
            {
                return GetNextMonthlyAbsoluteDate(dto, currentDate);
            }
            else if (dto.RepeatType == RepeaterTypeDto.MonthlyRelative)
            {
                return GetNextMonthlyRelativeDate(dto, currentDate);
            }
            else
            {
                return new ServiceResult<DateOnly>(DateOnly.MaxValue, ResultType.ClientError, ["Unable to locate RepeaterType"]);
            }
        }

        private static ServiceResult<DateOnly> GetNextWeeklyDate(IRepeatable dto, DateOnly startDate)
        {
            HashSet<DayOfWeek> daysOfWeek = [];

            dto.Repeats.ToList().ForEach(x => daysOfWeek.Add((DayOfWeek)x.DayIdentifier));

            while (!daysOfWeek.Contains(startDate.DayOfWeek))
            {
                startDate = startDate.AddDays(1);
            }

            return new ServiceResult<DateOnly>(startDate);
        }

        private static ServiceResult<DateOnly> GetNextMonthlyAbsoluteDate(IRepeatable dto, DateOnly startDate)
        {
            HashSet<int> daysOfMonth = [];

            dto.Repeats.ToList().ForEach(x => daysOfMonth.Add(x.DayIdentifier));

            var currentDay = startDate.Day;

            // If we're later in the month than all of the options, set to first (lowest) value for next month.
            if (daysOfMonth.All(x => x < currentDay))
            {
                var nextDate = new DateOnly(startDate.Year, startDate.Month, daysOfMonth.Min()).AddMonths(1);
                return new ServiceResult<DateOnly>(nextDate);
            }
            else
            {
                foreach (var day in daysOfMonth.Order())
                {
                    if (day >= currentDay)
                    {
                        var nextDate = new DateOnly(startDate.Year, startDate.Month, day);
                        return new ServiceResult<DateOnly>(nextDate);
                    }
                };
            }

            return ServiceResult<DateOnly>.DefaultServerFailure();
        }

        private static ServiceResult<DateOnly> GetNextMonthlyRelativeDate(IRepeatable dto, DateOnly startDate)
        {
            Dictionary<int, HashSet<DayOfWeek>> daysByWeek = new() {
                        { 1, []},
                        { 2, []},
                        { 3, []},
                        { -1, []},
                };

            dto.Repeats.Where(x => x.Index.HasValue).ToList().ForEach(x =>
            {
                if (daysByWeek.TryGetValue(x.Index!.Value, out var set))
                {
                    set.Add((DayOfWeek)x.DayIdentifier);
                }
                else
                {
                    // TODO logging, invalid Index value set
                }
            });

            // We should hit within a month in most cases, but limit the loop in case something is wrong.
            var currentDate = startDate;
            while (currentDate <= startDate.AddMonths(2))
            {
                var startOfMonth = new DateOnly(currentDate.Year, currentDate.Month, 1);
                var dateToCheck = startOfMonth;

                int index = 1;
                var daysOfWeek = daysByWeek[index];
                while (index <= 3)
                {
                    daysOfWeek = daysByWeek[index];
                    // Loop through the week to check
                    for (int i = 0; i < 7; i++)
                    {
                        if (dateToCheck >= startDate)
                        {
                            if (daysOfWeek.Contains(dateToCheck.DayOfWeek))
                            {
                                return new ServiceResult<DateOnly>(dateToCheck);
                            }
                        }
                        dateToCheck = dateToCheck.AddDays(1);
                    }

                    index++;
                }

                // No match in first 3 weeks, check final week.
                daysOfWeek = daysByWeek[-1];
                if (daysOfWeek.Count != 0)
                {
                    dateToCheck = startOfMonth.AddMonths(1).AddDays(-8); // Final week of the month
                    for (int i = 0; i < 7; i++)
                    {
                        if (dateToCheck >= startDate)
                        {
                            if (daysOfWeek.Contains(dateToCheck.DayOfWeek))
                            {
                                return new ServiceResult<DateOnly>(dateToCheck);
                            }
                        }

                        dateToCheck = dateToCheck.AddDays(1);
                    }
                }

                currentDate = currentDate.AddMonths(1);
            }

            // TODO logging - shouldn't get here
            return ServiceResult<DateOnly>.DefaultServerFailure();
        }

    }
}
