using Data.Entity.Appointments;

namespace Data.Interfaces
{
    public interface IAppointmentRepo
    {
        public ICollection<Appointment> GetAppointmentsBetweenDates(DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Get exceptions to time block sequences in a given range, grouped by time block guid
        /// </summary>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>All the exceptions occuring in the requested range, grouped by time block guid</returns>
        public Dictionary<Guid, ICollection<TimeBlockException>> GetTimeBlockExceptionsBetweenDates(DateOnly startDate, DateOnly endDate);

        public Task<ICollection<Appointment>> GetPendingAppointments();

        public Task<bool> Create(Appointment appointment);

        public Task<bool> Update(Appointment appointment);

        public Task<bool> DeleteAppointment(Guid id);

        public Task<ICollection<TimeBlock>> GetTimeBlocks();

        public Task<TimeBlock?> GetTimeBlock(Guid guid);

        public Task<bool> Create(TimeBlock timeBlock);

        public Task<bool> Update(TimeBlock timeBlock);

        public Task<bool> DeleteTimeBlock(Guid id, bool deleteExceptions);

        public Task<bool> Create(TimeBlockException exception, Guid timeBlockGuid);
        public Task<bool> Update(TimeBlockException exception, Guid timeBlockGuid);
        public Task<bool> DeleteException(TimeBlockException exception, Guid timeBlockGuid);
    }
}
