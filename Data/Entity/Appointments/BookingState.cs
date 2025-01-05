namespace Data.Entity.Appointments
{
    public enum BookingState : byte  // use byte as parent to ensure EF maps it to tinyint instead of int
    {
        Confirmed = 0,
        Pending = 1,
        Rejected = 2,
    }
}
