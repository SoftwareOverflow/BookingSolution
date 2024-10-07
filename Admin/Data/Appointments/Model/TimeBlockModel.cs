using Core.Dto.Appointment;

namespace Admin.Data.Appointments.Model
{
    internal class TimeBlockModel(TimeBlockDto dto) : BaseAppointmentModel<TimeBlockDto>(dto)
    {
        public RepeaterModel Repeater { get; set; } = new RepeaterModel(dto.RepeatType, dto.Repeats);

        public ICollection<TimeBlockExceptionDto> Exceptions
        {
            get => Appointment.Exceptions;
            set
            {
                Appointment.Exceptions = value;
            }
        }
    }
}
