﻿using Admin.Data.Events;
using Microsoft.VisualBasic;
using MudBlazor.Extensions;

namespace Admin.Data.Helpers
{
    public static class DateHelpers
    {
        private static DateTime StartOfWeek(DateTime date)
        {
            return date.StartOfWeek(
                System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek
            );
        }

        public static Tuple<DateTime, DateTime> GetDateRange(CalendarLayoutType layoutType, DateTime date)
        {

            var startDate = date;
            var endDate = date;

            // Cannot use switch statement, CalendarLayoutType enums are not compile time consts
            if (layoutType == CalendarLayoutType.Week)
            {
                startDate = StartOfWeek(date);
                endDate = startDate.AddDays(6);
            }
            else if (layoutType == CalendarLayoutType.WorkWeek)
            {
                startDate = StartOfWeek(date);
                endDate = startDate.AddDays(4);
            }
            else if (layoutType == CalendarLayoutType.Month)
            {
                // The month layout will contain some days from prior months at the
                // start or end. Adjust accordingly.
                startDate = new DateTime(date.Year, date.Month, date.Day);

                var startOfWeek = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                while (startDate.DayOfWeek != startOfWeek)
                {
                    startDate = startDate.AddDays(-1);
                }

                endDate = startDate.AddMonths(1).AddDays(-1);
                while (endDate.DayOfWeek != startOfWeek)
                {
                    endDate = endDate.AddDays(1);
                }
                endDate = endDate.AddDays(-1);
            }

            return new Tuple<DateTime, DateTime>(startDate.Date, endDate.Date);
        }

        public static string GetDateLabel(DateTime? date, CalendarLayoutType layout)
        {
            if (date == null)
            {
                return "No Date Selected";
            }

            // Cannot use switch here as CalendarLayoutType cannot be compile time constant
            if (layout == CalendarLayoutType.Day)
            {
                return date.Value.ToString("dd MMM yy");
            }

            else if (layout == CalendarLayoutType.Week)
            {
                var firstDay = StartOfWeek(date.Value);
                var lastDay = firstDay.AddDays(6);
                return $"{firstDay.ToString("dd MMM")} - {lastDay.ToString("dd MMM yy")}";
            }

            else if (layout == CalendarLayoutType.WorkWeek)
            {
                var firstDay = StartOfWeek(date.Value);
                var lastDay = firstDay.AddDays(4);
                return $"{firstDay.ToString("dd MMM")} - {lastDay.ToString("dd MMM yy")}";
            }

            else if (layout == CalendarLayoutType.Month)
            {
                return date.Value.ToString("MMMM yy");
            }

            else
            {
                return "";
            }
        }
    }
}
