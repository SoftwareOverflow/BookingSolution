using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entity.Appointments
{
    public class Appointment : BusinessControlledEntity
    {
        /// <summary>
        /// Name of the service - usually the name of the booked service
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Start of the booking, excluding any padding
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// End of the booking, excluding any padding
        /// </summary>
        public DateTime EndTime { get; set; }

        // TODO store some sort of status for the appointment
        // TODO this should be an id to a status table for normalization

        /// <summary>
        /// The type of the booking - e.g. online or manual
        /// </summary>       
        public BookingType BookingType { get; set; }

        /// <summary>
        /// Foregin Key to the <see cref="Person"/> who made the booking
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Foreign Key to the <see cref="Service"/> this booking was for
        /// </summary>
        public int? ServiceId { get; set; }

        public virtual Person Person { get; set; }

        public virtual Service? Service { get; set; }
    }
}
