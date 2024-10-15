using Core.Dto.Validation;
using System.ComponentModel.DataAnnotations;

namespace Core.Dto.Appointment
{
    public record TimeBlockDto(string Name) : AppointmentDtoBase(Name), IRepeatable
    {
        /// <summary>
        /// RepeatType for this TimeBlock.
        /// Null for a one-off TimeBlock.
        /// </summary>
        public RepeaterTypeDto? RepeatType { get; set; } = null;

        /// <summary>
        /// The repeats for the current TimeBlock
        /// </summary>
        public ICollection<RepeaterDto> Repeats { get; set; } = [];

        /// <summary>
        /// Any exceptions to the current repeater TimeBlock
        /// </summary>
        public ICollection<TimeBlockExceptionDto> Exceptions { get; set; } = [];
    }
}
