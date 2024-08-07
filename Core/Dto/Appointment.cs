namespace Core.Dto
{
    public record Appointment : DtoBase
    {
        public string Name { get; set; }

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
        public TimeSpan PaddingStart { get; set; }

        /// <summary>
        /// Time padding to be added after the event finishes
        /// </summary>
        public TimeSpan PaddingEnd { get; set; }
    }
}
