using Core.Dto.BookingRequest;
using System.ComponentModel.DataAnnotations;

namespace Admin.Data.Booking
{
    public class BookingRequestModel(BookingRequestDto bookingRequest)
    {
        [ValidateComplexType]
        public BookingRequestDto BookingRequest { get; set; } = bookingRequest;

        private DateTime? _dateTime = bookingRequest.SelectedDate.ToDateTime(new TimeOnly());

        [Required]
        public DateTime? SelectedDateTime
        {
            get => _dateTime;
            set
            {
                if (value.HasValue)
                {
                    BookingRequest.SelectedDate = DateOnly.FromDateTime(value.Value);
                }

                _dateTime = value;
            }
        }
    }
}
