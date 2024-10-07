namespace Data.Entity.Appointments
{
    /// <summary>
    /// Holds exceptions to the normal repeaters
    /// </summary>
    public class TimeBlockException : BaseAppointment
    {
        /// <summary>
        /// Foreign Keys to the assosciated TimeBlock recurring sequence.
        /// Nullable - the exception could still exist after the TimeBlock repeats have been deleted.
        /// </summary>
        public int? TimeBlockId { get; set; }

        /// <summary>
        /// This is the Start date of the TimeBlock which is being replaced with this event
        /// </summary>
        public DateOnly DateToReplace { get; set; }
    }
}
