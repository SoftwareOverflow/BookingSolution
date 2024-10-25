using Data.Entity.Appointments;

namespace Data.Interfaces
{
    public interface IAppointmentRepo
    {
        public ICollection<Appointment> GetAppointmentsBetweenDates(DateOnly startDate, DateOnly endDate);

        public Task<bool> Create(Appointment appointment);

        public Task<bool> Update(Appointment appointment);

        public Task<bool> DeleteAppointment(Guid id);

        public Task<ICollection<TimeBlock>> GetTimeBlocks();

        public Task<TimeBlock?> GetTimeBlock(Guid guid);

        public Task<bool> Create(TimeBlock timeBlock);

        public Task<bool> Update(TimeBlock timeBlock);

        public Task<bool> DeleteTimeBlock(Guid id);

        public Task<bool> Create(TimeBlockException exception);
        public Task<bool> Update(TimeBlockException exception);
        public Task<bool> DeleteException(TimeBlockException exception);
    }
}
