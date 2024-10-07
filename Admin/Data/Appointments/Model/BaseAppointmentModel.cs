using Core.Dto.Appointment;
using System.ComponentModel.DataAnnotations;

namespace Admin.Data.Appointments.Model
{
    internal abstract class BaseAppointmentModel<T>(T Appointment) where T : AppointmentDtoBase
    {
        [ValidateComplexType]
        public T Appointment { get; private set; } = Appointment;

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

        protected virtual void ValidateTimes() { }

        protected virtual void ValidateName() { }
    }
}
