using Core.Dto;
using Core.Dto.Appointment;

namespace Admin.Data.Appointments.Model
{
    internal class AppointmentModel : BaseAppointmentModel<AppointmentDto>
    {
        private readonly List<ServiceTypeDto> _services;

        public Guid? ServiceGuid
        {
            get => Appointment.Service?.Guid ?? null;
            set
            {

                if (value == null)
                {
                    Appointment.Service = null;
                    Errors.Remove(AppointmentModelError.ServiceNameMismatch);
                    Errors.Remove(AppointmentModelError.ServiceDurationMismatch);
                }
                else
                {
                    Appointment.Service = _services.SingleOrDefault(s => s.Guid == value) ?? Appointment.Service;
                    ValidateName();
                    ValidateTimes();
                }
            }
        }


        public AppointmentModel(AppointmentDto dto, List<ServiceTypeDto> services) : base(dto)
        {
            _services = services;

            ServiceGuid = Appointment.Service?.Guid;
            StartDate = Appointment.StartTime;
            EndDate = Appointment.EndTime;
            StartTime = Appointment.StartTime.TimeOfDay;
            EndTime = Appointment.EndTime.TimeOfDay;
        }

        protected override void ValidateName()
        {
            if (ServiceGuid != null && Appointment.Service?.Name != Appointment.Name)
            {
                Errors.Add(AppointmentModelError.ServiceNameMismatch);
            }
            else
            {
                Errors.Remove(AppointmentModelError.ServiceNameMismatch);
            }
        }

        protected override void ValidateTimes()
        {
            var duration = Appointment.Service?.DurationMins;

            if (duration != null)
            {
                var expectedEnd = Appointment.StartTime.AddMinutes(duration.Value);

                if (Appointment.EndTime != expectedEnd)
                {
                    Errors.Add(AppointmentModelError.ServiceDurationMismatch);
                }
                else
                {
                    Errors.Remove(AppointmentModelError.ServiceDurationMismatch);
                }
            }
            else
            {
                Errors.Remove(AppointmentModelError.ServiceDurationMismatch);
            }
        }

        public HashSet<AppointmentModelError> Errors { get; private set; } = [];
    }
}
