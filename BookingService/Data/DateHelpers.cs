using MudBlazor.Extensions;

namespace BookingService.Data
{
    public static class DateHelpers
    {
        public static DateTime StartOfWeek(DateTime date)
        {
            return date.StartOfWeek(
                System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek
            );
        }

        public static Tuple<DateTime, DateTime> GetDateRange(DateTime date)
        {

            var startDate = date;
            var endDate = date;

            // The month layout will contain some days from prior months at the
            // start or end. Adjust accordingly.
            startDate = new DateTime(date.Year, date.Month, 1);

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

            return new Tuple<DateTime, DateTime>(startDate.Date, endDate.Date);
        }

        public static DateTime JumpToNextMonth(DateTime? date, bool forward)
        {
            if (date == null)
            {
                return DateTime.Now;
            }

            return date.Value.AddMonths(forward ? 1 : -1);
        }
    }
}