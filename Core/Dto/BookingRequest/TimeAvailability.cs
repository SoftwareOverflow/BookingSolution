namespace Core.Dto.BookingRequest
{
    public class TimeAvailability(TimeOnly time)
    {
        public TimeOnly Time { get; set; } = time;

        public AvailabilityState State { get; set; } = AvailabilityState.Available;
    }
}
