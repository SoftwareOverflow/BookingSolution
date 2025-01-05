namespace Data.Entity.Appointments
{
    public class Appointment : BaseAppointment
    {
        // TODO store some sort of status for the appointment - id to a status table for normalization?

        /// <summary>
        /// The type of the booking - e.g. online or manual
        /// </summary>       
        public BookingType BookingType { get; set; }

        /// <summary>
        /// The State of the current booking - e.g. pending or confirmed
        /// </summary>
        public BookingState State { get; set; }

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
