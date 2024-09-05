using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.Dto.BookingRequest
{
    /// <summary>
    /// Used when requesting a new booking.
    /// Stores required information to request a booking.
    /// Will be mapped to a <see cref="AppointmentDto"/> upon submission
    /// </summary>
    public record BookingRequestDto
    {
        /// <summary>
        /// The Service
        /// </summary>
        [Required]
        public ServiceTypeDto Service { get; set; }

        /// <summary>
        /// The person making the booking request
        /// </summary>
        [ValidateComplexType]
        public PersonDto Person { get; set; }

        /// <summary>
        /// Date the booking is being requested
        /// </summary>
        [Required]
        [DisplayName("Date")]
        public DateOnly SelectedDate { get; set; }

        /// <summary>
        /// The time at which the booking is being requested
        /// </summary>
        [Required(ErrorMessage = "Time is required")]
        [DisplayName("Time")]
        public TimeOnly? SelectedTime { get; set; }

        public BookingRequestDto(ServiceTypeDto service, PersonDto person, DateOnly selectedDate)
        {
            Service = service;
            Person = person;
            SelectedDate = selectedDate;
        }
    }
}