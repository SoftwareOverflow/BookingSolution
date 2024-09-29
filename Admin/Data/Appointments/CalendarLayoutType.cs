namespace Admin.Data.Appointments
{
    public sealed class CalendarLayoutType
    {
        public static readonly CalendarLayoutType Day = new("Day");
        public static readonly CalendarLayoutType Week = new("Week");
        public static readonly CalendarLayoutType WorkWeek = new("Work Week");
        public static readonly CalendarLayoutType Month = new("Month");

        public static readonly List<CalendarLayoutType> Types =
        [
            Day, Week, WorkWeek, Month
        ];

        public string Name { get; private set; }
        private CalendarLayoutType(string name)
        {
            Name = name;
        }
    }
}
