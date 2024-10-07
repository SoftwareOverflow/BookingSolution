namespace Data.Entity.Appointments
{
    /// <summary>
    /// Blocked out time on the users calendar.
    /// During these times appointments cannot be booked.
    /// </summary>
    public class TimeBlock : BaseAppointment
    {        
        /// <summary>
        /// RepeatType for this TimeBlock.
        /// Null for a one-off TimeBlock.
        /// </summary>
        public RepeatType? RepeatType { get; set; }

        /// <summary>
        /// The repeats for the current TimeBlock
        /// </summary>
        public virtual ICollection<TimeBlockRepeater> Repeats { get; set; } = [];

        /// <summary>
        /// Any exceptions to the current repeater TimeBlock
        /// </summary>
        public virtual ICollection<TimeBlockException> Exceptions { get; set; } = [];
    }
}
