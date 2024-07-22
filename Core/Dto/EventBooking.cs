namespace Core.Dto
{
    public class EventBooking
    {
        public string Name { get; set; }

        public string? Location { get; set; }

        /// <summary>
        /// The StartTime of the booking, excluding any padding
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The EndTime of the booking, excluding any padding
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Time padding to be added before the event starts
        /// </summary>
        public TimeSpan EventPaddingStart { get; set; }

        /// <summary>
        /// Time padding to be added after the event finishes
        /// </summary>
        public TimeSpan EventPaddingEnd { get; set; }
    }
}
