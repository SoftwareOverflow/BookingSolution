using Core.Dto.Validation;
using System.ComponentModel.DataAnnotations;

namespace Core.Dto.Appointment
{
    public abstract record AppointmentDtoBase(string Name) : DtoBase
    {
        /// <summary>
        /// Name of the booking, usually the name of the booked service
        /// </summary>
        [Required]
        public string Name { get; set; } = Name;

        /// <summary>
        /// The StartTime of the booking, excluding any padding
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The EndTime of the booking, excluding any padding
        /// </summary>
        [DateMustBeAfter(nameof(StartTime))]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// CURRENTLY UNUSED - Time padding to be added before the event starts
        /// </summary>
        public TimeSpan PaddingStart { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// CURRENTLY UNUSED - Time padding to be added after the event finishes
        /// </summary>
        public TimeSpan PaddingEnd { get; set; } = TimeSpan.Zero;


        #region unmapped utility properties / methods
        
        /// <summary>
        /// The start time, accounting for padding
        /// </summary>
        public DateTime StartTimePadded => StartTime.Subtract(PaddingStart);

        /// <summary>
        /// The end time, accounting for padding
        /// </summary>
        public DateTime EndTimePadded => EndTime.Add(PaddingEnd);

        /// <summary>
        /// Get the StartTime for this booking on the requested date
        /// </summary>
        /// <param name="date">Date to get the start time.</param>
        /// <param name="padded">Wheter to include padding in the timing</param>
        /// <returns>The time of day at which this appointment starts on the requested date</returns>
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

        /// <summary>
        /// Get the EndTime for this booking on the requested date
        /// </summary>
        /// <param name="date">Date to get the end time.</param>
        /// <param name="padded">Wheter to include padding in the timing</param>
        /// <returns>The time of day at which this appointment ends on the requested date</returns>
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
