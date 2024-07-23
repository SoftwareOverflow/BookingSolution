namespace Admin.Data.Events
{
    public static class EventLayoutConsts
    {
        public readonly static int CellHeight = 36;

        public readonly static int EventsWidthPc = 85;

        private readonly static double CellHeightHour = EventLayoutConsts.CellHeight * 2f; // Each cell is 30 minutes
        public static double CellHeightMin => CellHeightHour / 60f;
    }
}
