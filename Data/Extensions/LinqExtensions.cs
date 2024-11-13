using Data.Entity.Appointments;

namespace Data.Extensions
{
    public static class LinqExtensions
    {
        public static IQueryable<T> BetweenDates<T>(this IQueryable<T> appointments, DateOnly startDate, DateOnly endDate) where T : BaseAppointment
        {
            return appointments.Where(x => x.StartTime <= endDate.ToDateTime(TimeOnly.MaxValue) && x.EndTime >= startDate.ToDateTime(TimeOnly.MinValue)
                    && x.StartTime != x.EndTime);
        }
    }
}
