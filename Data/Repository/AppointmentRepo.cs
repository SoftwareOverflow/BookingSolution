using Data.Context;
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

            await ExecuteVoidAsync(async (db, _) =>
            {
                var businesId = await db.GetBusinessId();
                appointment.BusinessId = businesId;

                if (await db.Appointments.AnyAsync(x => x.Guid == appointment.Guid))
                {
                    throw new ArgumentException("Unable to create appointment - id already exists");
                }

                if (appointment.Service != null)
                {
                    var service = db.Services.SingleOrDefault(s => s.Guid == appointment.Service!.Guid) ?? throw new ArgumentException("Cannot find linked service");
                    appointment.Service = service;
                }

                db.Appointments.Add(appointment);
                await db.SaveChangesAsync();
            });

            return true;
        }

        public async Task<bool> Update(Appointment appointment)
        {
            if (appointment.Person == null)
            {
                throw new ArgumentException("Unable create appointment - Person details required");
            }

            await ExecuteVoidAsync(async (db, _) =>
            {
                var existing = await db.Appointments.Include(a => a.Person).SingleOrDefaultAsync(x => x.Guid == appointment.Guid) ?? throw new ArgumentException("Unable to find existing appointment");

                // Manually map the foreign keys first to ensure we don't try and create any new objects
                appointment.Id = existing.Id;
                appointment.BusinessId = existing.BusinessId;
                appointment.PersonId = existing.PersonId;

                // Handle the Service
                if (appointment.Service != null)
                {
                    var service = await db.Services.SingleOrDefaultAsync(s => s.Guid == appointment.Service.Guid) ?? throw new ArgumentException("Unable to find linked service");

                    appointment.ServiceId = service.Id;
                    appointment.Service = null;
                }
                else
                {
                    appointment.ServiceId = null;
                }

                // Handle the Person - need to update separately due to SetValues only applying to top level entity.
                appointment.Person.Id = existing.PersonId;
                db.People.Entry(existing.Person).CurrentValues.SetValues(appointment.Person);
                db.People.Update(existing.Person);

                db.Appointments.Entry(existing).CurrentValues.SetValues(appointment);
                db.Appointments.Update(existing);

                await db.SaveChangesAsync();
            });

            return true;
        }

        public ICollection<Appointment> GetAppointmentsBetweenDates(DateOnly startDate, DateOnly endDate)
        {
            return Execute<ICollection<Appointment>>((db, _) =>
            {
                return db.Appointments.AsQueryable().BetweenDates(startDate, endDate).Include(a => a.Person).Include(a => a.Service).ToList();
            });
        }

        public async Task<bool> DeleteAppointment(Guid id)
        {
            await ExecuteVoidAsync(async (db, _) =>
            {
                var existing = await db.Appointments.Include(a => a.Person).SingleOrDefaultAsync(x => x.Guid == id) ?? throw new ArgumentException("Unable to find appointment");

                db.Appointments.Remove(existing);
                db.People.Remove(existing.Person);
                await db.SaveChangesAsync();

            });

            return true;
        }

        public async Task<ICollection<TimeBlock>> GetTimeBlocks()
        {
            return await ExecuteAsync<ICollection<TimeBlock>>(async (db, _) =>
            {
                return await db.TimeBlocks.Include(tb => tb.Repeats).Include(tb => tb.Exceptions).ToListAsync();
            }) ?? [];
        }

        public Task<TimeBlock?> GetTimeBlock(Guid guid)
        {
            return ExecuteAsync(async (db, _) => await db.TimeBlocks.Include(tb => tb.Exceptions).Include(tb => tb.Repeats).SingleOrDefaultAsync(x => x.Guid == guid));
        }

        public async Task<bool> Create(TimeBlock timeBlock)
        {
            await ExecuteVoidAsync(async (db, _) =>
            {
                var businesId = await db.GetBusinessId();
                timeBlock.BusinessId = businesId;

                if (await db.TimeBlocks.AnyAsync(x => x.Guid == timeBlock.Guid))
                {
                    throw new ArgumentException("Unable to create Time Block - id already exists");
                }

                db.TimeBlocks.Add(timeBlock);
                await db.SaveChangesAsync();
            });

            return true;
        }

        public async Task<bool> Update(TimeBlock timeBlock)
        {
            await ExecuteVoidAsync(async (db, _) =>
            {
                using var transaction = await db.Database.BeginTransactionAsync();

                var existing = await db.TimeBlocks.Include(tb => tb.Repeats).Include(tb => tb.Exceptions).SingleOrDefaultAsync(tb => tb.Guid == timeBlock.Guid) ?? throw new ArgumentException("Cannot find Time Block to update");

                // Manually map the foreign keys first to ensure we don't try and create any new objects
                timeBlock.Id = existing.Id;
                timeBlock.BusinessId = existing.BusinessId;

                // Check the repeats - updates to RepeatType or Repeaters are not allowed.
                if (existing.RepeatType != timeBlock.RepeatType || !timeBlock.Repeats.Equals(existing.Repeats))
                {
                    db.TimeBlockExceptions.RemoveRange(existing.Exceptions);
                    await db.SaveChangesAsync();
                }

                // The nested repeat objects do NOT get updated by default. Manually copy them
                existing.Repeats = timeBlock.Repeats;

                db.TimeBlocks.Entry(existing).CurrentValues.SetValues(timeBlock);

                await db.SaveChangesAsync();

                await transaction.CommitAsync();
            });

            return true;
        }

        public async Task<bool> DeleteTimeBlock(Guid id, bool deleteExceptions)
        {
            await ExecuteVoidAsync(async (db, _) =>
            {
                var timeBlock = db.TimeBlocks.Include(tb => tb.Exceptions).SingleOrDefault(tb => tb.Guid == id) ?? throw new ArgumentException("Cannot find time block");

                if (deleteExceptions)
                {
                    db.TimeBlockExceptions.RemoveRange(timeBlock.Exceptions);
                }
                else
                {
                    // If we're keeping the TimeBlock exceptions they need to be unlinked from the TimeBlock instance
                    foreach (var exception in timeBlock.Exceptions)
                    {
                        exception.TimeBlockId = null;
                    }
                }

                db.TimeBlocks.Remove(timeBlock);
                await db.SaveChangesAsync();
            });

            return true;
        }

        public async Task<bool> Create(TimeBlockException exception, Guid timeBlockGuid)
        {
            await ExecuteVoidAsync(async (db, _) =>
            {
                var businessId = await db.GetBusinessId();

                var timeBlock = db.TimeBlocks.Include(tb => tb.Exceptions).SingleOrDefault(tb => tb.Guid == timeBlockGuid) ?? throw new ArgumentException("Cannot find time block");
                var existing = timeBlock.Exceptions.SingleOrDefault(tbe => tbe.DateToReplace == exception.DateToReplace);
                if (existing != null)
                {
                    throw new ArgumentException("Time Block Exception exists - call Update instead.");
                }

                exception.BusinessId = businessId;
                exception.TimeBlockId = timeBlock.Id;

                db.TimeBlockExceptions.Add(exception);
                await db.SaveChangesAsync();
            });

            return true;
        }

        public async Task<bool> Update(TimeBlockException exception, Guid timeBlockGuid)
        {
            await ExecuteVoidAsync(async (db, _) =>
            {
                var businessId = await db.GetBusinessId();

                var timeBlock = db.TimeBlocks.Include(tb => tb.Exceptions).SingleOrDefault(tb => tb.Guid == timeBlockGuid) ?? throw new ArgumentException("Cannot find time block");
                var existing = timeBlock.Exceptions.SingleOrDefault(tbe => tbe.Guid == exception.Guid) ?? throw new ArgumentException("Cannot find time block exception");

                exception.BusinessId = businessId;
                exception.TimeBlockId = timeBlock.Id;
                exception.Id = existing.Id;

                db.TimeBlockExceptions.Entry(existing).CurrentValues.SetValues(exception);
                await db.SaveChangesAsync();
            });

            return true;
        }

        public async Task<bool> DeleteException(TimeBlockException exception, Guid timeBlockGuid)
        {
            // TODO consider adding a boolean for revertSequence or something, and then either delete or change the start and end times to match

            await ExecuteVoidAsync(async (db, _) =>
            {                
                var existing = db.TimeBlockExceptions.SingleOrDefault(tbe => tbe.Guid == exception.Guid);

                var newTime = new DateTime(DateOnly.FromDateTime(exception.StartTime), new TimeOnly(0, 0, 0));
                exception.StartTime = newTime;
                exception.EndTime = newTime;

                exception.Id = existing?.Id ?? exception.Id;
                exception.BusinessId = await db.GetBusinessId();

                if (existing == null)
                {
                    var timeBlock = db.TimeBlocks.SingleOrDefault(tb => tb.Guid == timeBlockGuid) ?? throw new ArgumentException("Cannot find time block");

                    // Create the new new exception
                    exception.TimeBlockId = timeBlock.Id;

                    existing = exception;
                    db.TimeBlockExceptions.Add(existing);
                }
                else
                {
                    // If the timeBlockId is null, the assosciated timeblock has already been separately deleted and so it's safe to delete the exception.
                    // Otherwise just set the duration to 0
                    if (existing.TimeBlockId == null)
                    {
                        db.TimeBlockExceptions.Remove(existing);
                    }
                    else
                    {
                        exception.TimeBlockId = existing.TimeBlockId;
                        db.TimeBlockExceptions.Entry(existing).CurrentValues.SetValues(exception);
                    }
                };

                await db.SaveChangesAsync();
            });

            return true;
        }

        public Dictionary<Guid, ICollection<TimeBlockException>> GetTimeBlockExceptionsBetweenDates(DateOnly startDate, DateOnly endDate)
        {
            return Execute((db, _) =>
            {
                var result = new Dictionary<Guid, ICollection<TimeBlockException>>();

                var exceptions = db.TimeBlockExceptions.AsQueryable().BetweenDates(startDate, endDate).ToList();

                foreach (var exception in exceptions)
                {
                    var timeBlockGuid = Guid.Empty;

                    if (exception.TimeBlockId != null)
                    {
                        var timeBlock = db.TimeBlocks.Find(exception.TimeBlockId);
                        if (timeBlock != null)
                        {
                            timeBlockGuid = timeBlock.Guid;
                        }
                        else
                        {
                            // TODO logging - shouldn't end up here 
                        }
                    }

                    if (result.TryGetValue(timeBlockGuid, out var list))
                    {
                        list.Add(exception);
                    }
                    else
                    {
                        result.Add(timeBlockGuid, [exception]);
                    }
                }

                return result;
            });
        }
    }
}
