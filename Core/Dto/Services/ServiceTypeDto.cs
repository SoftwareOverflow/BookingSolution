using Core.Dto.Services;
using Core.Dto.Validation;
using System.ComponentModel.DataAnnotations;

namespace Core.Dto
{
    public record ServiceTypeDto : DtoBase
    {
        /// <summary>
        /// The name of the service selected by the user
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; }


        /// <summary>
        /// The Service will be available to book from this date.
        /// </summary>
        [Required]
        public DateTime? StartDate { get; set; } = DateTime.Today;

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
        [RequireMinDuration(1, "Duration is required")]
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
        [Range(0.00, double.MaxValue, ErrorMessage = "Enter valid price")]
        public decimal Price { get; set; }

        public ServiceRepeaterTypeDto RepeatType { get; set; } = ServiceRepeaterTypeDto.Weekly;

        public ICollection<ServiceRepeaterDto> Repeats { get; set; } = [];

        //region mappings
        public int PaddingStartMins
        {
            get => (int)PaddingStart.TotalMinutes;
            set => PaddingStart = TimeSpan.FromMinutes(value);
        }

        public int PaddingEndMins
        {
            get => (int)PaddingEnd.TotalMinutes;
            set => PaddingEnd = TimeSpan.FromMinutes(value);
        }

        public int DurationMins
        {
            get => (int)Duration.TotalMinutes;
            set => Duration = TimeSpan.FromMinutes(value);
        }
        //endregion
    }
}
