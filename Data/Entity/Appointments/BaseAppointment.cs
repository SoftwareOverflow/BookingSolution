using System.ComponentModel.DataAnnotations;

namespace Data.Entity.Appointments
{
    public class BaseAppointment : BusinessControlledEntity
    {
        /// <summary>
        /// Name of the appointment - usually the name of the booked service
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
    }
}
