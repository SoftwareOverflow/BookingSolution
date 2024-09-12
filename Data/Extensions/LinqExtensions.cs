using Data.Entity.Appointments;

namespace Data.Extensions
{
    public static class LinqExtensions
    {
        public static IQueryable<Appointment> BetweenDates(this IQueryable<Appointment> appointments, DateOnly startDate, DateOnly endDate)
        {
            return appointments.Where(x => x.StartTime <= endDate.ToDateTime(TimeOnly.MaxValue) && x.EndTime >= startDate.ToDateTime(TimeOnly.MinValue));
        }
    }
}
