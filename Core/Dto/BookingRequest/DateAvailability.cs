namespace Core.Dto.BookingRequest
{
    public class DateAvailability(DateOnly date)
    {
        public DateOnly Date { get; set; } = date;

        public List<TimeAvailability> Times { get; set; } = [];
    }
}
