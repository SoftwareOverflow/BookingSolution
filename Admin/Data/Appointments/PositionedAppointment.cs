using Core.Dto;

namespace Admin.Data.Events
{
    public class PositionedAppointment
    {
        public Appointment Event { get; set; }

        /// <summary>
        /// Contains clash information for each date of the event.
        /// As events can span across multiple days, it is possible to have different
        /// clashes each day
        /// </summary>
        private Dictionary<DateOnly, AppointmentClash> ClashDict = [];

        public PositionedAppointment(Appointment booking)
        {
            Event = booking;
        }

        public DateOnly GetStartDate(bool padded = false)
        {
            var startDate = padded ? Event.StartTime.Subtract(Event.PaddingStart) : Event.StartTime;
            return DateOnly.FromDateTime(startDate);
        }

        public DateOnly GetEndDate(bool padded = false)
        {
            var end = padded ? Event.EndTime.Add(Event.PaddingEnd) : Event.EndTime;
            return DateOnly.FromDateTime(end);
        }

        protected internal TimeOnly GetStartTime(DateOnly date, bool padded)
        {
            var start = padded ? Event.StartTime.Subtract(Event.PaddingStart) : Event.StartTime;
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
            var end = padded ? Event.EndTime.Add(Event.PaddingEnd) : Event.EndTime;
            var endDate = DateOnly.FromDateTime(end);

            var start = padded ? Event.StartTime.Subtract(Event.PaddingStart) : Event.StartTime;
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

        private double GetDurationMins(DateOnly date, bool padded)
        {
            var start = GetStartTime(date, padded).ToTimeSpan();
            var end = GetEndTime(date, padded).ToTimeSpan();

            return end.Subtract(start).TotalMinutes;
        }

        public int TopPx(DateOnly date, bool padded = false)
        {
            var start = GetStartTime(date, padded);

            int top = (int)(start.ToTimeSpan().TotalMinutes * AppointmentLayoutConsts.CellHeightMin);

            return top;
        }

        public int HeightPx(DateOnly date, bool padded = false) =>
            (int)(GetDurationMins(date, padded) * AppointmentLayoutConsts.CellHeightMin);

        public int WidthPc(DateOnly date)
        {
            if (ClashDict.TryGetValue(date, out var eventClash))
            {
                return (int)(AppointmentLayoutConsts.EventsWidthPc / (eventClash.Clashes + 1f));
            }
            else
            {
                return AppointmentLayoutConsts.EventsWidthPc;
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
            if (Event.PaddingEnd.TotalMinutes == 0 &&
                Event.PaddingStart.TotalMinutes == 0)
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
            ClashDict[date] = new AppointmentClash
            {
                Position = position,
                Clashes = clashes
            };
        }

        public void AddClash(DateOnly date)
        {
            ClashDict.TryGetValue(date, out var clash);
            clash ??= new AppointmentClash();

            clash.Clashes++;

            ClashDict[date] = clash;
        }

        /// <summary>
        /// Returns the top in px of the actual event compared to the padding
        /// </summary>
        public int GetRelativeTopPx(DateOnly date)
            => TopPx(date, false) - TopPx(date, true);

        /// <summary>
        /// Returns the height in % of the event without padding, to the event with padding. 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public int GetRelativeHeightPc(DateOnly date){

            if(GetEndTime(date, false) >= new TimeOnly(23, 59))
            {
                return 100;
            }

            double totalHeight = GetDurationMins(date, true);
            double unpadded = GetDurationMins(date, false);

            return (int)((unpadded * 100f / (totalHeight * 100f)) * 100f);
        }
    }
}