namespace Data.Entity.Appointments
{
    /// <summary>
    /// Type of the booking
    /// </summary>
    public enum BookingType : byte  // use byte as parent to ensure EF maps it to tinyint instead of int
    {
        Online = 0,
        Manual = 1,
    }
}
