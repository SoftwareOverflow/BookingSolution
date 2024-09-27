using Core.Dto;
using System.ComponentModel.DataAnnotations;

namespace Admin.Data.Appointments.Model
{
    public class AppointmentModel
    {
        [ValidateComplexType]
        public AppointmentDto Appointment { get; private set; }

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

        private DateTime? _startDate, _endDate;

        private TimeSpan? _startTime, _endTime;

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;

                if (_startDate != null)
                {
                    Appointment.StartTime = new DateTime(DateOnly.FromDateTime(_startDate.Value), TimeOnly.FromTimeSpan(Appointment.StartTime.TimeOfDay));
                    ValidateTimes();
                }
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;

                if (_endDate != null)
                {
                    Appointment.EndTime = new DateTime(DateOnly.FromDateTime(_endDate.Value), TimeOnly.FromTimeSpan(Appointment.EndTime.TimeOfDay));
                    ValidateTimes();
                }
            }
        }

        public TimeSpan? StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;

                if (_startTime != null)
                {
                    Appointment.StartTime = new DateTime(DateOnly.FromDateTime(Appointment.StartTime), TimeOnly.FromTimeSpan(_startTime.Value));
                    ValidateTimes();
                }
            }
        }

        public TimeSpan? EndTime
        {
            get => _endTime;
            set
            {
                _endTime = value;

                if (_endTime != null)
                {
                    Appointment.EndTime = new DateTime(DateOnly.FromDateTime(Appointment.EndTime), TimeOnly.FromTimeSpan(_endTime.Value));
                    ValidateTimes();
                }
            }
        }

        [Required]
        public string Name
        {
            get => Appointment.Name;
            set
            {
                Appointment.Name = value;
                ValidateName();
            }
        }

        public AppointmentModel(AppointmentDto dto, List<ServiceTypeDto> services)
        {
            Appointment = dto;
            _services = services;

            ServiceGuid = Appointment.Service?.Guid;
            StartDate = Appointment.StartTime;
            EndDate = Appointment.EndTime;
            StartTime = Appointment.StartTime.TimeOfDay;
            EndTime = Appointment.EndTime.TimeOfDay;
        }

        private void ValidateName()
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

        private void ValidateTimes()
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
