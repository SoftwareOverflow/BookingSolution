namespace Core.Dto.Appointment
{
    public record TimeBlockExceptionDto(string Name) : AppointmentDtoBase(Name)
    {
        /// <summary>
        /// This is the Start date of the TimeBlock which is being replaced with this event
        /// </summary>
        public DateOnly DateToReplace { get; set; }
    }
}
