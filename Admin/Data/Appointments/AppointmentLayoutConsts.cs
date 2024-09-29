namespace Admin.Data.Appointments
{
    public static class AppointmentLayoutConsts
    {
        public readonly static int CellHeight = 36;

        public readonly static int EventsWidthPc = 90;

        private readonly static double _cellHeighHour = CellHeight * 2f; // Each cell is 30 minutes
        public static double CellHeightMin => _cellHeighHour / 60f;
    }
}
