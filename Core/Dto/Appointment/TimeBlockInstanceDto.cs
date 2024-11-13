namespace Core.Dto.Appointment
{
    public record TimeBlockInstanceDto(Guid TimeBlockGuid, string Name, DateOnly Date, bool IsException = false, bool IsOneOff = false) : AppointmentDtoBase(Name)
    {
        /// <summary>
        /// Guid of the parent TimeBlock sequence
        /// </summary>
        public Guid TimeBlockGuid { get; private set; } = TimeBlockGuid;

        /// <summary>
        /// The date of this instance, primarily used for exceptions
        /// </summary>
        public DateOnly InstanceDate = Date;

        /// <summary>
        /// Flag indicating if this instance is an exception.
        /// </summary>
        public bool IsException = IsException;

        /// <summary>
        /// Flag indicating if this instance is a one-off time block
        /// </summary>
        public bool IsOneOff = IsOneOff;
    }
}
