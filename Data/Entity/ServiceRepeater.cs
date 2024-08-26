namespace Data.Entity
{
    public class ServiceRepeater : BaseEntity
    {
        /// <summary>
        /// Foreign key to the <see cref="Service"/>
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// Identifies the day on which to repeat.
        /// For repeats identified by a day of the week, this is represented by 0 (Sunday) to 6 (Saturday)
        /// For repeats identified by a specific day (e.g. MonthlyAbsolute) this is the day of the month.
        /// </summary>
        public int DayIdentifier { get; set; }


        /// <summary>
        /// The index for the repition.
        /// Should be null for most use cases, and is only used for <see cref="ServiceRepeatType.MonthlyRelative"/>
        /// In the case of MonthlyRelative it indicates which week in the month to be used.
        /// 1 -> First occurance in the month
        /// 2 -> Second occurance in the month
        /// 3 -> Third occurance in the month
        /// -1 -> Last occurance of the month
        /// </summary>
        public int? Index { get; set; } = null;
    }
}
