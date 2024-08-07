using System.ComponentModel.DataAnnotations;

namespace Data.Entity
{
    public class Service : BusinessControlledEntity
    {
        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The earliest time of day this can be booked
        /// </summary>
        public TimeOnly EarliestTime { get; set; }

        /// <summary>
        /// The latest time of day this can be booked
        /// </summary>
        public TimeOnly LatestTime { get; set; }

        /// <summary>
        /// The time between available slots e.g.
        /// 15 -> service would be available to book every 15 mins
        /// from <see cref="EarliestTime"/> up to <see cref="LatestTime"/>
        /// </summary>
        public int BookingFrequencyMins { get; set; }

        /// <summary>
        /// Length of bookings made using Service
        /// </summary>
        public int DurationMins { get; set; }

        /// <summary>
        /// Additional time before appointment blocked out, for preparation etc.
        /// </summary>
        public int PaddingStartMins { get; set; }

        /// <summary>
        /// Additional time blocked dout post event, for clearup etc
        /// </summary>
        public int PaddingEndMins { get; set; }

        /// <summary>
        /// Cost to the appointment maker
        /// </summary>
        public decimal Price { get; set; }


        #region AvailableDays 
        /// <summary>
        /// If the service is available on Monday
        /// </summary>
        public bool AvailableMonday { get; set; }

        /// <summary>
        /// If the service is available on Tuesday
        /// </summary>
        public bool AvailableTuesday { get; set; }

        /// <summary>
        /// If the service is available on Wednesday
        /// </summary>
        public bool AvailableWednesday { get; set; }

        /// <summary>
        /// If the service is available on Thursday
        /// </summary>
        public bool AvailableThursday { get; set; }

        /// <summary>
        /// If the service is available on Friday
        /// </summary>
        public bool AvailableFriday { get; set; }

        /// <summary>
        /// If the service is available on Saturday
        /// </summary>
        public bool AvailableSaturday { get; set; }

        /// <summary>
        /// If the service is available on Sunday
        /// </summary>
        public bool AvailableSunday { get; set; }
        #endregion
    }
}
