namespace Core.Dto
{
    public record AppointmentDto(string Name, PersonDto Person) : DtoBase
    {
        /// <summary>
        /// Name of the booked service
        /// </summary>
        public string Name { get; set; } = Name;

        /// <summary>
        /// The person who made the booking
        /// </summary>
        public PersonDto Person { get; set; } = Person;

        /// <summary>
        /// The service which was booked, or null if it can no longer be found
        /// </summary>
        public ServiceTypeDto? Service { get; set; } = null;

        /// <summary>
        /// The StartTime of the booking, excluding any padding
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The EndTime of the booking, excluding any padding
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Time padding to be added before the event starts
        /// </summary>
        public TimeSpan PaddingStart { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Time padding to be added after the event finishes
        /// </summary>
        public TimeSpan PaddingEnd { get; set; } = TimeSpan.Zero;


        #region unmapped utility properties / methods
        public DateTime StartTimePadded => StartTime.Subtract(PaddingStart);

        public DateTime EndTimePadded => EndTime.Add(PaddingEnd);

        public TimeOnly GetStartTime(DateOnly date, bool padded)
        {
            var start = padded ? StartTimePadded : StartTime;
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

        public TimeOnly GetEndTime(DateOnly date, bool padded)
        {
            var end = padded ? EndTimePadded : EndTime;
            var endDate = DateOnly.FromDateTime(end);

            var start = padded ? StartTimePadded : StartTime;
            var startDate = DateOnly.FromDateTime(start);

            if (date == endDate)
            {
                return TimeOnly.FromDateTime(end);
            }
            else if (date < startDate || date > endDate)
            {
                // Does not exist on this day
                return new TimeOnly(0, 0);
            }
            else
            {
                // Fills the whole day - ends at midnight
                return new TimeOnly(23, 59, 59);
            }
        }
        #endregion
    }
}
