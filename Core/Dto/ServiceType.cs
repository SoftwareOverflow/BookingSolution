using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace Core.Dto
{
    public class ServiceType
    {
        /// <summary>
        /// The name of the service selected by the user
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// A description of the offered service
        /// </summary>
        [StringLength(250)]
        public string Description { get; set; }

        /// <summary>
        /// Which days of the week this service is available
        /// </summary>
        public Dictionary<DayOfWeek, bool> DaysOffered { get; set; } = new Dictionary<DayOfWeek, bool>
        {
            {DayOfWeek.Monday, true },
            {DayOfWeek.Tuesday, true },
            {DayOfWeek.Wednesday, true },
            {DayOfWeek.Thursday, true },
            {DayOfWeek.Friday, true },
            {DayOfWeek.Saturday, false },
            {DayOfWeek.Sunday, false }
        };

        /// <summary>
        /// The earliest available booking time
        /// </summary>
        public TimeSpan? EarliestTime { get; set; } = new TimeSpan(9, 0, 0);

        /// <summary>
        /// The latest available booking time
        /// </summary>
        public TimeSpan? LatestTime { get; set; } = new TimeSpan(17, 0, 0);

        /// <summary>
        /// Backing property for <see cref="BookingFrequencyMins"/>
        /// </summary>
        private TimeSpan BookingFrequency = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Represents gap between offered booking slots.
        /// For example:
        /// 15 => 10:00, 10:15, 10:30 etc
        /// 30 => 10:00, 10:30, 11:00 etc
        /// 60 => 10:00, 11:00, 12:00 etc
        /// </summary>
        public int BookingFrequencyMins
        {
            get
            {
                return (int)BookingFrequency.TotalMinutes;
            }
            set
            {
                BookingFrequency = TimeSpan.FromMinutes(value);
            }
        }

        /// <summary>
        /// The Duration Of the Booking
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Padding at the start of the booking
        /// </summary>
        public TimeSpan PaddingStart { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Padding at the end of the booking
        /// </summary>
        public TimeSpan PaddingEnd { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// The price of the provided service
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Enter valid price")]
        public decimal Price { get; set; }
    }
}
