namespace Data.Entity.Appointments
{
    /// <summary>
    /// Type of the booking
    /// </summary>
    public enum BookingType : byte  // use byte as parent to ensure EF maps it to tinyint instead of int
    {
        ONLINE = 0,
        MANUAL = 1,
    }
}
