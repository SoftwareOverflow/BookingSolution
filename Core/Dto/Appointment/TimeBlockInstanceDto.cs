namespace Core.Dto.Appointment
{
    public record TimeBlockInstanceDto(Guid TimeBlockGuid, string Name, DateOnly Date, bool IsException = false) : AppointmentDtoBase(Name)
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
    }
}
