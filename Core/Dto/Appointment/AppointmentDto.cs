using System.ComponentModel.DataAnnotations;

namespace Core.Dto.Appointment
{
    public record AppointmentDto(string Name, PersonDto Person) : AppointmentDtoBase(Name)
    {
        /// <summary>
        /// The person who made the booking
        /// </summary>
        [ValidateComplexType]
        public PersonDto Person { get; set; } = Person;

        /// <summary>
        /// The service which was booked, or null if it can no longer be found
        /// </summary>
        public ServiceTypeDto? Service { get; set; } = null;


        public BookingTypeDto BookingType { get; set; }

        public BookingStateDto State { get; set; }
    }
}
