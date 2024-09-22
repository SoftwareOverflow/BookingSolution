﻿using Data.Context;
using Data.Entity.Appointments;
using Data.Extensions;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository
{
    internal class AppointmentRepo(IDbContextFactory<ApplicationDbContext> factory) : BaseRepo(factory), IAppointmentRepo
    {
        public async Task<bool> Create(Appointment appointment)
        {
            if (appointment.Person == null)
            {
                throw new ArgumentException("Unable create appointment - Person details required");
            }

            await ExecuteVoidAsync(async (db) =>
            {
                var businesId = await db.GetBusinessId();
                appointment.BusinessId = businesId;

                if (await db.Appointments.AnyAsync(x => x.Guid == appointment.Guid))
                {
                    throw new ArgumentException("Unable to create appointment - id already exists");
                }

                if (appointment.Service != null)
                {
                    var service = db.Services.SingleOrDefault(s => s.Guid == appointment.Service!.Guid);
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

                db.Appointments.Add(appointment);
                await db.SaveChangesAsync();
            });

            return true;
        }

        public async Task<bool> Update(Appointment appointment)
        {
            await ExecuteVoidAsync(async (db) =>
            {
                var existing = await db.Appointments.Include(a => a.Person).SingleOrDefaultAsync(x => x.Guid == appointment.Guid) ?? throw new ArgumentException("Unable to find existing appointment");

                // Manually map the foreign keys first to ensure we don't try and create any new objects
                appointment.Id = existing.Id;
                appointment.BusinessId = existing.BusinessId;
                appointment.PersonId = existing.PersonId;

                // Handle the Service
                if(appointment.Service != null)
                {
                    var service = await db.Services.SingleOrDefaultAsync(s => s.Guid == appointment.Service.Guid) ?? throw new ArgumentException("Unable to find linked service");

                    appointment.ServiceId = service.Id;
                    appointment.Service = null;
                } else
                {
                    appointment.ServiceId = null;
                }

                // Handle the Person - need to update separately due to SetValues only applying to top level entity.
                appointment.Person.Id = existing.PersonId;
                db.Person.Entry(existing.Person).CurrentValues.SetValues(appointment.Person);
                db.Person.Update(existing.Person);

                db.Appointments.Entry(existing).CurrentValues.SetValues(appointment);
                db.Appointments.Update(existing);

                await db.SaveChangesAsync();
            });

            return true;
        }

        public ICollection<Appointment> GetAppointmentsBetweenDates(DateOnly startDate, DateOnly endDate)
        {
            return Execute<ICollection<Appointment>>((db) =>
            {
                return db.Appointments.AsQueryable().BetweenDates(startDate, endDate).Include(a => a.Person).Include(a => a.Service).ToList();
            });
        }

        public async Task<bool> DeleteAppointment(Guid id)
        {
            await ExecuteVoidAsync(async (db) =>
            {
                var existing = await db.Appointments.SingleOrDefaultAsync(x => x.Guid == id) ?? throw new ArgumentException("Unable to find appointment");

                db.Appointments.Remove(existing);
                await db.SaveChangesAsync();

            });
            return true;
        }
    }
}
