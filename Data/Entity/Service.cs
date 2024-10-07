using Data.Entity.Appointments;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entity
{
    public class Service : BusinessControlledEntity
    {
        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// When the service is available for booking
        /// </summary>
        [Required]
        public DateOnly StartDate { get; set; }

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
        /// from <see cref="EarliestTime"/> such that it completes (<see cref="DurationMins") by /> <see cref="LatestTime"/>
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
        /// Additional time blocked out post event, for clearup etc
        /// </summary>
        public int PaddingEndMins { get; set; }

        /// <summary>
        /// Cost to the appointment maker
        /// </summary>
        [Column(TypeName = "decimal(19, 4)")]
        public decimal Price { get; set; }

        /// <summary>
        /// RepeatType for this Service
        /// </summary>
        public RepeatType RepeatType { get; set; }

        /// <summary>
        /// All the Repeats for this Service
        /// </summary>
        public virtual ICollection<ServiceRepeater> Repeats { get; set; } = [];

        /// <summary>
        /// Appointments made for this service
        /// </summary>
        public virtual ICollection<Appointment> Appointments { get; set; } = [];
    }
}
