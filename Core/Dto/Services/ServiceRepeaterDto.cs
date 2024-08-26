using Data.Entity;

namespace Core.Dto
{
    public record ServiceRepeaterDto : DtoBase
    {
        /// <summary>
        /// Identifies the day for this repeat (0-Sunday through 6-Saturday), or the day of the month in case of MonthlyAbsolute
        /// </summary>
        public int DayIdentifier { get; set; }

        /// <summary>
        /// Index only used for MonthlyRelative to indicate which occurance of the DayIdentifier is required
        /// </summary>
        public int? Index { get; set; } = null;

        public ServiceRepeaterDto(int dayIdentifier, int? index = null)
        {
            DayIdentifier = dayIdentifier;
            Index = index;
        }

        public ServiceRepeaterDto(DayOfWeek dayIdentifier, int? index = null)
        {
            DayIdentifier = (int)dayIdentifier;
            Index = index;
        }
    }
}
