namespace Admin.Data.Events
{
    /// <summary>
    /// Holds information to help layout with clashing events.
    /// </summary>
    public class AppointmentClash
    {
        /// <summary>
        /// The Position (index) of this event
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// The number of other events this event clashes with
        /// </summary>
        public int Clashes { get; set; }
    }
}
