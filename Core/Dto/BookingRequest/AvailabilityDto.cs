namespace Core.Dto.BookingRequest
{
    public class AvailabilityDto
    {
        public ICollection<DateAvailability> Availability { get; set; } = [];
    }
}
