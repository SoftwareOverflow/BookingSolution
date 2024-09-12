using Data.Entity.Appointments;

namespace Data.Interfaces
{
    public interface IAppointmentContext
    {
        public ICollection<Appointment> GetAppointmentsBetweenDates(DateOnly startDate, DateOnly endDate);

        public Task<bool> Create(Appointment appointment);

        public Task<bool> Update(Appointment appointment);

        public Task<bool> DeleteAppointment(Guid id);

    }
}
