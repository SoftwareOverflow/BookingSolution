namespace Data.Entity
{
    public enum RepeatType : byte  // use byte as parent to ensure EF maps it to tinyint instead of int
    {
        /// <summary>
        /// Repeats on the same weekday every week
        /// </summary>
        Weekly = 1,

        /// <summary>
        /// Repeats on the same day of the month (e.g. 7th)
        /// </summary>
        MonthlyAbsolute = 2,

        /// <summary>
        /// Repeats on the same week of the month (e.g. the second Tuesday of every month)
        /// </summary>
        MonthlyRelative = 3
    }
}
