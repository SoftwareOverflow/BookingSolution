using MudBlazor;

namespace Admin.Data.Appointments.Model
{
    public class AppointmentModelError(string message, Action<AppointmentModel>? fix = null, Severity severity = Severity.Info)
    {
        public string Message { get; private set; } = message;

        public Action<AppointmentModel>? Fix { get; private set; } = fix;

        public MudBlazor.Severity Severity { get; private set; } = severity;

        public readonly static AppointmentModelError ServiceNameMismatch = new("Name does not match the selected service", (x =>
        {
            x.Name = x.Appointment.Service?.Name ?? x.Name;
        }));

        public readonly static AppointmentModelError ServiceDurationMismatch = new("Appointment duration does not match the selected service");
    }
}
