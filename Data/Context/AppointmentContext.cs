using Data.Entity;
using Data.Entity.Appointments;
using Data.Extensions;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    internal partial class ApplicationDbContext : IAppointmentContext
    {
        private DbSet<Appointment> Appointments { get; set; }

        private DbSet<Person> Person { get; set; }

        public async Task<bool> Create(Appointment appointment)
        {
            if (appointment.Person == null)
            {
                throw new ArgumentException("Unable create appointment - Person details required");
            }

            var businesId = await GetBusinessId();
            appointment.BusinessId = businesId;

            if (await Appointments.AnyAsync(x => x.Guid == appointment.Guid))
            {
                throw new ArgumentException("Unable to create appointment - id already exists");
            }

            if (appointment.Service != null)
            {
                var service = Services.SingleOrDefault(s => s.Guid == appointment.Service!.Guid);
                if (service != null)
                {
                    appointment.Service = service; // Make sure the service matches, including Id, otherwise EF will try and create a new one.
                }
                else
                {
                    appointment.Service = null;
                    // TODO logging - shouldn't get to the position where we can't match the service
                }
            }

            Appointments.Add(appointment);
            await SaveChangesAsync();

            return true;
        }

        public async Task<bool> Update(Appointment appointment)
        {
            var existing = await Appointments.SingleOrDefaultAsync(x => x.Guid == appointment.Guid) ?? throw new ArgumentException("Unable to find existing appointment");

            // Manually map the foreign keys first
            appointment.BusinessId = existing.BusinessId;
            appointment.ServiceId = existing.ServiceId;
            appointment.PersonId = existing.PersonId;

            Appointments.Entry(existing).CurrentValues.SetValues(appointment);
            Appointments.Update(existing);

            await SaveChangesAsync();

            return true;
        }

        public ICollection<Appointment> GetAppointmentsBetweenDates(DateOnly startDate, DateOnly endDate)
        {
            return Appointments.AsQueryable().BetweenDates(startDate, endDate).Include(a => a.Person).Include(a => a.Service).ToList();
        }

        public async Task<bool> DeleteAppointment(Guid id)
        {
            var existing = await Appointments.SingleOrDefaultAsync(x => x.Guid == id) ?? throw new ArgumentException("Unable to find appointment");

            Appointments.Remove(existing);
            await SaveChangesAsync();

            return true;
        }
    }
}
