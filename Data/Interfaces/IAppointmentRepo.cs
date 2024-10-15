using Data.Entity.Appointments;

namespace Data.Interfaces
{
    public interface IAppointmentRepo
    {
        public ICollection<Appointment> GetAppointmentsBetweenDates(DateOnly startDate, DateOnly endDate);

        public Task<bool> Create(Appointment appointment);

        public Task<bool> Update(Appointment appointment);

        public Task<bool> DeleteAppointment(Guid id);

        public Task<bool> Create(TimeBlock timeBlock);

        public Task<bool> Update(TimeBlock timeBlock);

        public Task<bool> DeleteTimeBlock(Guid id);
    }
}
