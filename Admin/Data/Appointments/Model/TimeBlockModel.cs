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

        public TimeBlockDto MapRepeats()
        {
            if (Repeater.RepeatType != null)
            {
                Appointment.Repeats = Repeater.MapToDto();
                Appointment.RepeatType = Repeater.RepeatType!.Value;
            }
            else
            {
                Appointment.RepeatType = null;
                Appointment.Repeats = [];
            }

            return Appointment;
        }
    }
}
