using Core.Dto;

namespace Admin.Data.Events
{
    public class PositionedEventBooking
    {
        public EventBooking Event { get; set; }

        /// <summary>
        /// Contains clash information for each date of the event.
        /// As events can span across multiple days, it is possible to have different
        /// clashes each day
        /// </summary>
        private Dictionary<DateOnly, EventClash> ClashDict = [];

        public PositionedEventBooking(EventBooking booking)
        {
            Event = booking;
        }

        public DateOnly GetStartDate(bool padded = false)
        {
            var startDate = padded ? Event.StartTime.Subtract(Event.EventPaddingStart) : Event.StartTime;
            return DateOnly.FromDateTime(startDate);
        }

        public DateOnly GetEndDate(bool padded = false)
        {
            var end = padded ? Event.EndTime.Add(Event.EventPaddingEnd) : Event.EndTime;
            return DateOnly.FromDateTime(end);
        }

        protected internal TimeOnly GetStartTime(DateOnly date, bool padded)
        {
            var start = padded ? Event.StartTime.Subtract(Event.EventPaddingStart) : Event.StartTime;
            var startDate = DateOnly.FromDateTime(start);


            if (startDate == date)
            {
                return TimeOnly.FromDateTime(start);
            }
            else
            {
                // This includes both days within the event and outside the event
                return new TimeOnly(0, 0);
            }
        }

        protected internal TimeOnly GetEndTime(DateOnly date, bool padded)
        {
            var end = padded ? Event.EndTime.Add(Event.EventPaddingEnd) : Event.EndTime;
            var endDate = DateOnly.FromDateTime(end);

            var start = padded ? Event.StartTime.Subtract(Event.EventPaddingStart) : Event.StartTime;
            var startDate = DateOnly.FromDateTime(start);

            if (date == endDate)
            {
                return TimeOnly.FromDateTime(end);
            }
            else if (date < startDate || date > endDate)
            {

                return new TimeOnly(0, 0);
            }
            else
            {
                return new TimeOnly(23, 59, 59);
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

            int top = (int)(start.ToTimeSpan().TotalMinutes * EventLayoutConsts.CellHeightMin);

            return top;
        }

        public int HeightPx(DateOnly date, bool padded = false) =>
            (int)(GetDurationMins(date, padded) * EventLayoutConsts.CellHeightMin);

        public int WidthPc(DateOnly date)
        {
            if (ClashDict.TryGetValue(date, out var eventClash))
            {
                return (int)(EventLayoutConsts.EventsWidthPc / (eventClash.Clashes + 1f));
            }
            else
            {
                return EventLayoutConsts.EventsWidthPc;
            }
        }

        public int LeftPc(DateOnly date)
        {
            if (ClashDict.TryGetValue(date, out var eventClash))
            {
                return WidthPc(date) * eventClash.Position;
            }
            else
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

        public void AddClash(DateOnly date, int position, int clashes)
        {
            ClashDict[date] = new EventClash
            {
                Position = position,
                Clashes = clashes
            };
        }

        public void AddClash(DateOnly date)
        {
            ClashDict.TryGetValue(date, out var clash);
            clash ??= new EventClash();

            clash.Clashes++;

            ClashDict[date] = clash;
        }
    }
}