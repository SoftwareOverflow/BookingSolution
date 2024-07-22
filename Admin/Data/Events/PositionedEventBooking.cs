using Core.Dto;

namespace Admin.Data.Events
{
    public class PositionedEventBooking
    {
        public EventBooking Event { get; set; }

        private readonly double CellHeightHour = EventLayoutConsts.CellHeight * 2f; // Each cell is 30 minutes
        private double CellHeightMin => CellHeightHour / 60f;

        /// <summary>
        /// Contains clash information for each date of the event.
        /// As events can span across multiple days, it is possible to have different
        /// clashes each day
        /// </summary>
        private Dictionary<DateOnly, EventClash> ClashDict;

        public PositionedEventBooking(EventBooking booking, Dictionary<DateOnly, EventClash> clashes)
        {
            Event = booking;
            ClashDict = clashes;
        }

        private TimeOnly GetStartTime(DateOnly date, bool padded)
        {
            var start = padded ? Event.StartTime.Subtract(Event.EventPaddingStart) : Event.StartTime;

            if (DateOnly.FromDateTime(start) != date)
            {
                return new TimeOnly(0, 0);
            }
            else
            {
                return TimeOnly.FromDateTime(start);
            }
        }

        private TimeOnly GetEndTime(DateOnly date, bool padded)
        {
            var end = padded ? Event.EndTime.Add(Event.EventPaddingEnd) : Event.EndTime;

            if (DateOnly.FromDateTime(end) != date)
            {
                return new TimeOnly(23, 59, 59);
            }
            else
            {
                return TimeOnly.FromDateTime(end);
            }
        }

        private int GetDurationMins(DateOnly date, bool padded)
        {
            var start = GetStartTime(date, padded).ToTimeSpan();
            var end = GetEndTime(date, padded).ToTimeSpan();

            return (int)end.Subtract(start).TotalMinutes;
        }

        public int TopPx(DateOnly date, bool padded = false)
        {
            var start = GetStartTime(date, padded);

            int top = (int)((start.Hour * CellHeightHour) + (start.Minute * CellHeightMin));

            return top;
        }

        public int HeightPx(DateOnly date, bool padded = false) =>
            (int)(GetDurationMins(date, padded) * CellHeightMin);

        public int WidthPc(DateOnly date)
        {
            if (ClashDict.TryGetValue(date, out var eventClash))
            {
                return (int)(85f / (eventClash.Clashes + 1f));
            }
            else
            {
                return 85;
            }
        }

        public int LeftPc(DateOnly date)
        {
            if(ClashDict.TryGetValue(date, out var eventClash))
            {
                return WidthPc(date) * eventClash.Position;
            } else
            {
                return 0;
            }
        }

        public bool HasPadding(DateOnly date)
        {
            if (Event.EventPaddingEnd.TotalMinutes == 0 &&
                Event.EventPaddingStart.TotalMinutes == 0)
                return false;

            var startDate = DateOnly.FromDateTime(Event.StartTime);
            var endDate = DateOnly.FromDateTime(Event.EndTime);

            if (startDate == date || endDate == date)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}