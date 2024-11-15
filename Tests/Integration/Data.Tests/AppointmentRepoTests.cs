using Data.Entity.Appointments;
using Data.Repository;
using Data.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace Data.Tests
{
    public class AppointmentRepoTests : BaseUserChangeTestClass
    {
        private readonly AppointmentRepo _repo;
        private readonly AppointmentRepo _localRepo;

        public AppointmentRepoTests(DockerSqlFixture fixture) : base(fixture)
        {
            _repo = new AppointmentRepo(fixture);
            _localRepo = new AppointmentRepo(this);
        }

        [Fact]
        public void GetAppointmentsBetweenDates_Empty()
        {
            var result = _repo.GetAppointmentsBetweenDates(DateOnly.MaxValue, DateOnly.MaxValue);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAppointmentsBetweenDates()
        {
            using var context = GetContext();

            // Create some appointments far in the future so that we don't clash with existing ones.
            var business = context.BusinessUsers.Include(bu => bu.Business).ThenInclude(b => b.Appointments).Single(bu => bu.UserId == DockerSqlFixture.UserId).Business;
            var initialAptsCount = business.Appointments.Count();

            var serviceIds = await context.Services.Select(s => s.Id).ToListAsync();

            var mockStart = DateOnly.FromDateTime(DateTime.Today).AddYears(2);
            var mockEnd = mockStart.AddDays(7);

            List<Appointment> apts = SeedData.GetAppointments(7, 0);

            // wholly before start
            apts[0].StartTime = new DateTime(mockStart.AddDays(-7), new TimeOnly(9, 0));
            apts[0].EndTime = new DateTime(mockStart.AddDays(-7), new TimeOnly(17, 30));
            apts[0].ServiceId = serviceIds[Random.Shared.Next(serviceIds.Count)];

            // stars before, end on start
            apts[1].StartTime = new DateTime(mockStart.AddDays(-1), new TimeOnly(9, 0));
            apts[1].EndTime = new DateTime(mockStart, new TimeOnly(17, 30));

            //starts before start, ends before end
            apts[2].StartTime = new DateTime(mockStart.AddDays(-7), new TimeOnly(9, 0));
            apts[2].EndTime = new DateTime(mockStart.AddDays(1), new TimeOnly(17, 30));
            apts[2].ServiceId = serviceIds[Random.Shared.Next(serviceIds.Count)];

            // Start before, end after end
            apts[3].StartTime = new DateTime(mockStart.AddDays(-7), new TimeOnly(9, 0));
            apts[3].EndTime = new DateTime(mockEnd.AddDays(2), new TimeOnly(17, 30));

            // wholly inside
            apts[4].StartTime = new DateTime(mockStart.AddDays(1), new TimeOnly(9, 0));
            apts[4].EndTime = new DateTime(mockStart.AddDays(1), new TimeOnly(17, 30));
            apts[4].ServiceId = serviceIds[Random.Shared.Next(serviceIds.Count)];

            // straddles end
            apts[5].StartTime = new DateTime(mockEnd, new TimeOnly(9, 0));
            apts[5].EndTime = new DateTime(mockEnd.AddDays(2), new TimeOnly(17, 30));

            //wholly past end
            apts[6].StartTime = new DateTime(mockEnd.AddDays(1), new TimeOnly(9, 0));
            apts[6].EndTime = new DateTime(mockEnd.AddDays(2), new TimeOnly(17, 30));
            apts[6].ServiceId = serviceIds[Random.Shared.Next(serviceIds.Count)];

            business.Appointments.AddRange(apts);
            await context.SaveChangesAsync();

            // Check we can load all appointments
            var allAppointments = _repo.GetAppointmentsBetweenDates(DateOnly.MinValue, DateOnly.MaxValue);
            Assert.Equal(initialAptsCount + 7, allAppointments.Count);

            //Check we can get a subset
            var result = _repo.GetAppointmentsBetweenDates(mockStart, mockEnd);
            Assert.Equal(5, result.Count);
            //Check we don't have the ones wholly outside
            Assert.DoesNotContain(result, r => r.Guid == apts[0].Guid);
            Assert.DoesNotContain(result, r => r.Guid == apts[6].Guid);

            // Check the straddlers
            Assert.Contains(result, r => r.Guid == apts[1].Guid);
            Assert.Contains(result, r => r.Guid == apts[5].Guid);
        }

        [Fact]
        public void GetAppointmentsBetweenDates_NoUser()
        {
            MockUserId = "";

            var ex = Assert.ThrowsAny<Exception>(() => _localRepo.GetAppointmentsBetweenDates(DateOnly.MinValue, DateOnly.MinValue));
            Assert.Contains("user", ex.Message);
        }

        [Fact]
        public void GetAppointmentsBetweenDates_UserNoBusiness()
        {
            MockUserId = Guid.NewGuid().ToString();

            var result = _localRepo.GetAppointmentsBetweenDates(DateOnly.MinValue, DateOnly.MaxValue);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Delete()
        {
            Guid toDelete = Guid.Empty;
            int personId = -1, initialAptsCount = -1, allPeopleCount = -1, serviceId = -1;
            using (var context = GetContext())
            {
                var initialApts = context.BusinessUsers.Include(b => b.Business).ThenInclude(b => b.Appointments).ThenInclude(a => a.Person).Single(bu => bu.UserId == DockerSqlFixture.UserId).Business.Appointments;
                initialAptsCount = initialApts.Count;
                allPeopleCount = context.People.IgnoreQueryFilters().Count();

                var toDeleteObj = initialApts.OrderBy(a => a.Guid).First(a => a.ServiceId != null);
                toDelete = toDeleteObj.Guid;
                personId = toDeleteObj.PersonId;
                serviceId = toDeleteObj.ServiceId!.Value;
            }

            var result = await _repo.DeleteAppointment(toDelete);

            Assert.True(result);

            using (var context = GetContext())
            {
                var newApts = context.BusinessUsers.Include(b => b.Business).ThenInclude(b => b.Appointments).ThenInclude(a => a.Person).Single(bu => bu.UserId == DockerSqlFixture.UserId).Business.Appointments;
                var newPeople = await context.People.IgnoreQueryFilters().ToListAsync();

                var services = await context.Services.ToListAsync();

                Assert.Equal(initialAptsCount - 1, newApts.Count);
                Assert.DoesNotContain(newApts, a => a.Guid == toDelete);

                //Ensure we have deleted the person assosciated
                Assert.Equal(allPeopleCount - 1, newPeople.Count);
                Assert.DoesNotContain(newPeople, p => p.Id == personId);

                //Ensure we did NOT delete the assosciated service
                Assert.Contains(services, s => s.Id == serviceId);
            }
        }

        [Fact]
        public async Task Delete_NoUser()
        {
            MockUserId = "";

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _localRepo.DeleteAppointment(Guid.NewGuid()));
            Assert.Contains("user", ex.Message);
        }

        [Fact]
        public async Task Delete_UserNoBusiness()
        {
            MockUserId = Guid.NewGuid().ToString();

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _localRepo.DeleteAppointment(Guid.NewGuid()));
            Assert.Contains("appointment", ex.Message);
        }

        [Fact]
        public async Task Delete_GuidMismatch()
        {
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.DeleteAppointment(Guid.NewGuid()));
            Assert.Contains("appointment", ex.Message);
        }

        [Fact]
        public async Task Create_WithService()
        {
            var toCreate = SeedData.GetAppointments(1, 0).Single();
            toCreate.ServiceId = null;

            int initialCount = -1, servicesCount = -1, expectedServiceId;
            using (var context = GetContext())
            {
                var service = context.Services.OrderBy(s => s.Guid).Last();
                toCreate.Service = service;
                expectedServiceId = service.Id;

                initialCount = context.Appointments.Count();
                servicesCount = context.Services.Count();
            }

            var result = await _repo.Create(toCreate);

            Assert.True(result);

            using (var context = GetContext())
            {
                var services = context.Services.ToList();
                var apts = context.Appointments.ToList();

                var created = apts.Single(a => a.Guid == toCreate.Guid);

                // Check we have a new apt, but NO new service
                Assert.Equal(initialCount + 1, apts.Count);
                Assert.Equal(servicesCount, services.Count);
                Assert.NotEqual(0, toCreate.BusinessId);
                Assert.Equal(expectedServiceId, created.ServiceId);
            }
        }

        [Fact]
        public async Task Create_NoService()
        {
            var toCreate = SeedData.GetAppointments(1, 0).Single();
            toCreate.ServiceId = null;
            toCreate.Service = null;

            int initialCount = -1, servicesCount = -1;
            using (var context = GetContext())
            {
                initialCount = context.Appointments.Count();
                servicesCount = context.Services.Count();
            }

            var result = await _repo.Create(toCreate);

            Assert.True(result);

            using (var context = GetContext())
            {
                var services = context.Services.ToList();
                var apts = context.Appointments.ToList();

                var created = apts.Single(a => a.Guid == toCreate.Guid);

                // Check we have a new apt, but NO new service
                Assert.Equal(initialCount + 1, apts.Count);
                Assert.Equal(servicesCount, services.Count);
                Assert.NotEqual(0, toCreate.BusinessId);
                Assert.Null(created.ServiceId);
            }
        }

        [Fact]
        public async Task Create_GuidExists()
        {
            var toCreate = SeedData.GetAppointments(1, 0).Single();

            using var context = GetContext();
            var existing = context.Appointments.OrderBy(x => x.Id).First();
            toCreate.Guid = existing.Guid;

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Create(toCreate));
            Assert.Contains("appointment", ex.Message);
        }

        [Fact]
        public async Task Create_ServiceNotFound()
        {
            var toCreate = SeedData.GetAppointments(1, 0).Single();
            toCreate.ServiceId = 0;
            toCreate.Service = SeedData.GetServices(1).Single();
            toCreate.Service.Guid = Guid.NewGuid();

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Create(toCreate));
            Assert.Contains("service", ex.Message);
        }

        [Fact]
        public async Task Create_NoPerson()
        {
            var toCreate = new Appointment();

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Create(toCreate));

            Assert.Contains("Person", ex.Message);
        }

        [Fact]
        public async Task Create_NoUser()
        {
            MockUserId = "";

            var toCreate = SeedData.GetAppointments(1, 0).Single();

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _localRepo.Create(toCreate));
            Assert.Contains("user", ex.Message);
        }

        [Fact]
        public async Task Create_UserNoBusiness()
        {
            MockUserId = Guid.NewGuid().ToString();

            var toCreate = SeedData.GetAppointments(1, 0).Single();

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _localRepo.Create(toCreate));
            Assert.Contains("business", ex.Message);
        }

        [Fact]
        public async Task Update_ServiceNotFound()
        {
            using var context = GetContext();
            var toUpdate = context.Appointments.Include(a => a.Person).OrderBy(a => a.Guid).Last();
            toUpdate.ServiceId = 0;
            toUpdate.Service = SeedData.GetServices(1).Single();
            toUpdate.Service.Guid = Guid.NewGuid();

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Update(toUpdate));
            Assert.Contains("service", ex.Message);
        }

        [Fact]
        public async Task Update_NoPerson()
        {
            using var context = GetContext();

            var toUpdate = new Appointment();
            toUpdate.Guid = context.Appointments.OrderBy(a => a.Guid).First().Guid;

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Update(toUpdate));

            Assert.Contains("Person", ex.Message);
        }

        [Fact]
        public async Task Update_NoUser()
        {
            MockUserId = "";

            using var context = GetContext();
            var toUpdate = context.Appointments.Include(a => a.Person).OrderBy(a => a.Guid).First();

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _localRepo.Update(toUpdate));
            Assert.Contains("user", ex.Message);
        }

        [Fact]
        public async Task Update_UserNoBusiness()
        {
            MockUserId = Guid.NewGuid().ToString();

            using var context = GetContext();
            var toUpdate = context.Appointments.OrderBy(a => a.Guid).First();

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _localRepo.Update(toUpdate));
            Assert.Contains("appointment", ex.Message);
        }

        [Fact]
        public async Task Update_GuidMismatch()
        {
            using var context = GetContext();
            var toUpdate = context.Appointments.Include(a => a.Person).OrderBy(a => a.Guid).First();
            toUpdate.Guid = Guid.NewGuid();

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Update(toUpdate));
            Assert.Contains("appointment", ex.Message);
        }

        [Fact]
        public async Task Update_ChangeService()
        {
            var personId = -1;

            Appointment toUpdate = new Appointment();
            using (var context = GetContext())
            {
                toUpdate = context.Appointments.Include(a => a.Person).OrderBy(a => a.Guid).First(a => a.Service != null);
                personId = toUpdate.PersonId;

                toUpdate.Service = context.Services.OrderBy(s => s.Guid).First(s => s.Id != toUpdate.ServiceId);

                // Change other params
                toUpdate.Name = "My new custom name";
                toUpdate.BookingType = BookingType.Online;
                toUpdate.EndTime = DateTime.Now.AddYears(5);
                toUpdate.StartTime = DateTime.Now.AddYears(5);
                toUpdate.Person.PhoneNumber = "01234546789";
                toUpdate.Person.EmailAddress = "oddExampleAaa@oddExampleAaa.com";
                toUpdate.Person.FirstName = "Test First Name";
                toUpdate.Person.LastName = "Test Last Name";
            }

            var result = await _repo.Update(toUpdate);

            Assert.True(result);

            using (var context = GetContext())
            {
                var apt = context.Appointments.Include(a => a.Person).Single(a => a.Id == toUpdate.Id);

                // Check that the new values match our updated version
                Assert.Equivalent(toUpdate, apt);
            }
        }

        [Fact]
        public async Task Update_RemoveService()
        {
            var personId = -1;

            Appointment toUpdate = new Appointment();
            using (var context = GetContext())
            {
                toUpdate = context.Appointments.Include(a => a.Person).OrderBy(a => a.Guid).First(a => a.Service != null);
                personId = toUpdate.PersonId;

                toUpdate.Service = null; // Remove the service

                // Change other params
                toUpdate.Name = "New name";
                toUpdate.BookingType = BookingType.Manual;
                toUpdate.EndTime = DateTime.Now.AddYears(4);
                toUpdate.StartTime = DateTime.Now.AddYears(4);
                toUpdate.Person.PhoneNumber = "0112233445566778899";
                toUpdate.Person.EmailAddress = "justsomeemailaddress@randomdomain.com";
                toUpdate.Person.FirstName = "Updated first name";
                toUpdate.Person.LastName = "Updated last name";
            }

            var result = await _repo.Update(toUpdate);

            Assert.True(result);

            using (var context = GetContext())
            {
                var apt = context.Appointments.Include(a => a.Person).Single(a => a.Id == toUpdate.Id);

                Assert.Null(apt.ServiceId);
                // Check that the new values match our updated version
                Assert.Equivalent(toUpdate, apt);
            }
        }

        [Fact]
        public async Task GetTimeBlocks()
        {
            List<TimeBlock> timeBlocks = [];
            TimeBlock tbUpdated = new TimeBlock();

            using (var context = GetContext())
            {
                timeBlocks = context.TimeBlocks.Include(tb => tb.Exceptions).Include(tb => tb.Repeats).ToList();

                tbUpdated = timeBlocks.OrderBy(tb => tb.Id).First();
                tbUpdated.Repeats = [new() { DayIdentifier = 1 }, new() { DayIdentifier = 2 }];
                tbUpdated.Exceptions = [new() { Name = "New Name", DateToReplace = new DateOnly(2024, 10, 1), BusinessId = tbUpdated.BusinessId},
                    new() { Name = "New Name 2", DateToReplace = new DateOnly(2024, 11, 1), BusinessId = tbUpdated.BusinessId},
                    new() { Name = "New Name 3", DateToReplace = new DateOnly(2024, 11, 2), BusinessId = tbUpdated.BusinessId},
                ];

                await context.SaveChangesAsync();
            }

            var result = await _repo.GetTimeBlocks();

            Assert.Equal(timeBlocks.Count, result.Count);
            Assert.All(result, r => timeBlocks.Single(tb => r.Guid == tb.Guid));

            var resultTb = result.Single(tb => tb.Id == tbUpdated.Id);
            Assert.Equal(2, resultTb.Repeats.Count);
            Assert.Equal(3, resultTb.Exceptions.Count);
        }

        [Fact]
        public async Task GetTimeBlock()
        {
            var timeBlocks = SeedData.GetTimeBlocks(10, 0);


            using (var context = GetContext())
            {
                var business = context.BusinessUsers.Include(bu => bu.Business).ThenInclude(b => b.Appointments).Single(bu => bu.UserId == DockerSqlFixture.UserId).Business;

                timeBlocks[5].Repeats = [
                new() {
                    DayIdentifier = (int)DayOfWeek.Monday
                },
                new() {
                    DayIdentifier = (int)DayOfWeek.Tuesday,

                }
                ];
                timeBlocks[5].Exceptions = [
                    new() {
                    Name = "Exceptional",
                    DateToReplace = DateOnly.MaxValue,
                    StartTime = timeBlocks[5].StartTime.AddDays(-24),
                    EndTime = timeBlocks[5].EndTime.AddDays(-24),
                    BusinessId = business.Id
                }];

                business.TimeBlocks.AddRange(timeBlocks);
                await context.SaveChangesAsync();
            }

            var result = await _repo.GetTimeBlock(timeBlocks[5].Guid);


            Assert.Equivalent(timeBlocks[5].Id, result!.Id);
            Assert.Equivalent(timeBlocks[5].Name, result!.Name);
            Assert.Equivalent(timeBlocks[5].StartTime, result.StartTime);
            Assert.Equivalent(timeBlocks[5].EndTime, result.EndTime);
            Assert.Equivalent(timeBlocks[5].RepeatType, result!.RepeatType);
            Assert.Equivalent(timeBlocks[5].Repeats, result!.Repeats);

            foreach (var tbe in timeBlocks[5].Exceptions)
            {
                Assert.Single(result!.Exceptions, r => r.Guid == tbe.Guid &&
                                    r.Name == tbe.Name &&
                                    r.StartTime == tbe.StartTime &&
                                    r.EndTime == tbe.EndTime &&
                                    r.DateToReplace == tbe.DateToReplace);
            }
        }

        [Fact]
        public async Task GetTimeBlock_NoneReturnsVoid()
        {
            var result = await _repo.GetTimeBlock(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTimeBlockExceptionInstances()
        {
            var mockStart = new DateOnly(2024, 10, 1);
            var mockEnd = new DateOnly(2024, 10, 30);

            TimeBlock[] tbs = new TimeBlock[10];
            TimeBlockException[] tbes = new TimeBlockException[6];
            using (var context = GetContext())
            {
                var business = context.BusinessUsers.Include(bu => bu.Business).ThenInclude(b => b.TimeBlocks).Single(bu => bu.UserId == DockerSqlFixture.UserId).Business;

                tbs = business.TimeBlocks.OrderBy(tb => tb.Id).ToArray();

                // Time Block starts after
                tbs[1].StartTime = new DateTime(mockEnd.AddDays(1), new TimeOnly(9, 0));
                tbs[1].EndTime = new DateTime(mockEnd.AddDays(2), new TimeOnly(17, 30));

                // Weekly with instances in range
                tbs[2].StartTime = new DateTime(2024, 10, 3, 10, 30, 0);
                tbs[2].EndTime = new DateTime(2024, 10, 3, 12, 30, 0);
                tbs[2].RepeatType = Entity.RepeatType.Weekly;
                tbs[2].Repeats = [new() { DayIdentifier = (int)DayOfWeek.Wednesday }]; //9th, (16th -> 17th), (23rd -> DEL), 30th
                tbs[2].Exceptions = [
                    // Move 16th to 17th
                    new() { Name = "ABC", BusinessId = business.Id, DateToReplace = new DateOnly(2024, 10, 16), StartTime = new DateTime(2024, 10, 17, 10, 30, 0), EndTime = new DateTime(2024, 10, 17, 12, 30, 0) },
                // Delete 23rd
                new() { Name = "DEF", BusinessId = business.Id, DateToReplace = new DateOnly(2024, 10, 23), StartTime = new DateTime(2024, 10, 23, 0, 0, 0), EndTime = new DateTime(2024, 10, 23, 0, 0, 0) }
                    ];

                // MonthlyAbsoulte with instances in range
                tbs[3].StartTime = new DateTime(2024, 10, 3, 10, 30, 0);
                tbs[3].EndTime = new DateTime(2024, 10, 3, 12, 30, 0);
                tbs[3].RepeatType = Entity.RepeatType.MonthlyAbsolute;
                tbs[3].Repeats = [new() { DayIdentifier = 10 }, new() { DayIdentifier = 11 }, new() { DayIdentifier = 12 }]; // (10th -> 13th), (11th -> DEL), 12th
                tbs[3].Exceptions = [
                    // Move 10th to 13th
                    new() { Name = "ABCD", BusinessId = business.Id, DateToReplace = new DateOnly(2024, 10, 10), StartTime = new DateTime(2024, 10, 13, 1, 30, 0), EndTime = new DateTime(2024, 10, 13, 1, 45, 0) },
                // Delete 11th
                new() { Name = "ABCDE", BusinessId = business.Id, DateToReplace = new DateOnly(2024, 10, 11), StartTime = new DateTime(2024, 10, 11, 0, 0, 0), EndTime = new DateTime(2024, 10, 11, 0, 0, 0) }
                    ];

                // MonthlyRelative with instances in range
                tbs[4].StartTime = new DateTime(2024, 10, 3, 10, 30, 0);
                tbs[4].EndTime = new DateTime(2024, 10, 3, 12, 30, 0);
                tbs[4].RepeatType = Entity.RepeatType.MonthlyRelative;
                tbs[4].Repeats = [
                    new() { DayIdentifier = (int) DayOfWeek.Monday, Index = 1 }, // 1st Monday (7th)
                new() { DayIdentifier = (int)DayOfWeek.Monday, Index = 2 }, // 2nd Monday (14th)
                new() { DayIdentifier = (int)DayOfWeek.Monday, Index = 3 }, // 3rd Monday (21th)
                new() { DayIdentifier = (int) DayOfWeek.Wednesday, Index = -1 }]; // Last Wed (23rd)
                tbs[4].Exceptions = [
                    // Move November into range
                    new() { Name = "XYZ", BusinessId = business.Id, DateToReplace = new DateOnly(2024, 11, 4), StartTime = new DateTime(2024, 10, 1, 11, 30, 0), EndTime = new DateTime(2024, 10, 1, 11, 45, 0) },
                // Move 7th -> 9th
                new() { Name = "XYZZ", BusinessId = business.Id, DateToReplace = new DateOnly(2024, 10, 7), StartTime = new DateTime(2024, 10, 9, 0, 0, 0), EndTime = new DateTime(2024, 10, 9, 6, 0, 0) },
                // Delete 23rd
                new() { Name = "DBA", BusinessId = business.Id, DateToReplace = new DateOnly(2024, 10, 23), StartTime = new DateTime(2024, 10, 23, 0, 0, 0), EndTime =  new DateTime(2024, 10, 23, 0, 0, 0) }
                    ];

                // Time Block starts after but exception inside
                tbs[5].StartTime = new DateTime(2024, 11, 1, 9, 0, 0);
                tbs[5].EndTime = new DateTime(2024, 11, 1, 17, 30, 0);
                tbs[5].RepeatType = Entity.RepeatType.Weekly;
                tbs[5].Repeats = [new() { DayIdentifier = (int)DayOfWeek.Friday }];
                tbs[5].Exceptions = [
                    new() {
                        Name = "GHI",
                        BusinessId = business.Id,
                        DateToReplace = new DateOnly(2024, 11, 8),
                        StartTime = new DateTime(2024, 10, 11, 9, 30, 0),
                        EndTime = new DateTime(2024, 10, 11, 10, 30, 0),
                }
                ];

                // Exceptions without assosciated time blocks - ie the TB has been deleted.
                tbes =
                [
                    new()
                    {
                        Name = "No TimeBlock #1",
                        BusinessId = business.Id,
                        DateToReplace = new DateOnly(2024, 8, 17), // Before still before
                        StartTime = new DateTime(2024, 9, 9, 20, 0, 0),
                        EndTime = new DateTime(2024, 9, 9, 22, 0, 0),
                    },

                    new()
                    {
                        Name = "No TimeBlock #2",
                        BusinessId = business.Id,
                        DateToReplace = new DateOnly(2024, 8, 17), // Before now after
                        StartTime = new DateTime(2024, 11, 9, 20, 0, 0),
                        EndTime = new DateTime(2024, 11, 9, 22, 0, 0),
                    },

                    new()
                    {
                        Name = "No TimeBlock #3",
                        BusinessId = business.Id,
                        DateToReplace = new DateOnly(2024, 9, 15), // Before into range
                        StartTime = new DateTime(2024, 10, 9, 20, 0, 0),
                        EndTime = new DateTime(2024, 10, 9, 22, 0, 0),
                    },

                    new()
                    {
                        Name = "No TimeBlock #4",
                        BusinessId = business.Id,
                        DateToReplace = new DateOnly(2024, 11, 15), // After into range
                        StartTime = new DateTime(2024, 10, 15, 20, 0, 0),
                        EndTime = new DateTime(2024, 10, 15, 22, 0, 0),
                    },

                    new()
                    {
                        Name = "No TimeBlock #5",
                        BusinessId = business.Id,
                        DateToReplace = new DateOnly(2024, 11, 15), // After now before
                        StartTime = new DateTime(2024, 8, 15, 20, 0, 0),
                        EndTime = new DateTime(2024, 8, 15, 22, 0, 0),
                    },

                    new()
                    {
                        Name = "No TimeBlock #6",
                        BusinessId = business.Id,
                        DateToReplace = new DateOnly(2024, 11, 15), // After still after
                        StartTime = new DateTime(2024, 12, 15, 20, 0, 0),
                        EndTime = new DateTime(2024, 12, 15, 22, 0, 0),
                    },

                    new()
                    {
                        Name = "No TimeBlock #7",
                        BusinessId = business.Id,
                        DateToReplace = new DateOnly(2024, 10, 15), // In Range buy empty
                        StartTime = new DateTime(2024, 10, 15, 20, 0, 0),
                        EndTime = new DateTime(2024, 10, 15, 20, 0, 0),
                    },

                ];

                context.TimeBlockExceptions.AddRange(tbes);

                await context.SaveChangesAsync();
            }

            var expected = new Dictionary<Guid, ICollection<TimeBlockException>>
            {
                { tbs[2].Guid, tbs[2].Exceptions.Where(e => e.DateToReplace.Day == 16).ToList() },
                { tbs[3].Guid,  tbs[3].Exceptions.Where(e => e.DateToReplace.Day == 10).ToList() },
                { tbs[4].Guid,  tbs[4].Exceptions.Where(e => e.DateToReplace.Day == 4 || e.DateToReplace.Day == 7).ToList() },
                { tbs[5].Guid, tbs[5].Exceptions.Where(e => e.DateToReplace.Day == 8).ToList() },
                { Guid.Empty,  [tbes[2], tbes[3]] },
            };


            var result = _repo.GetTimeBlockExceptionsBetweenDates(mockStart, mockEnd);
            Assert.Equal(expected.Values.Sum(x => x.Count), result.Values.Sum(r => r.Count));

            foreach (var kvp in result)
            {
                Assert.Contains(expected.Keys, k => k == kvp.Key);

                var expectedValues = expected[kvp.Key];
                Assert.Equal(expectedValues.Count, kvp.Value.Count);

                foreach (var ev in expectedValues)
                {
                    Assert.Contains(kvp.Value, k => k.DateToReplace == ev.DateToReplace &&
                                                    k.StartTime == ev.StartTime &&
                                                    k.EndTime == ev.EndTime &&
                                                    k.Guid == ev.Guid);
                }
            }
        }

        [Fact]
        public async Task CreateTimeBlock_IdExists()
        {
            using var context = GetContext();

            var existing = context.TimeBlocks.OrderByDescending(x => x.Id).First();

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Create(new TimeBlock() { Guid = existing.Guid }));
            Assert.Contains("exists", ex.Message);
        }

        [Fact]
        public async Task CreateTimeBlock()
        {
            var newId = 0;
            try
            {
                using var context = GetContext();
                var existing = context.TimeBlocks.ToList();

                var toCreate = new TimeBlock()
                {
                    Name = "Newly Created",
                    RepeatType = Entity.RepeatType.Weekly,
                    Repeats = [new() { DayIdentifier = 0 }, new() { DayIdentifier = 1 }],
                    Exceptions = [] // Cannot have exceptions when initially creating a timeblock
                };

                var result = await _repo.Create(toCreate);

                newId = toCreate.Id;

                Assert.True(result);
                Assert.Equal(await context.GetBusinessId(), toCreate.BusinessId);
                Assert.Equal(existing.Count + 1, context.TimeBlocks.ToList().Count);

                Assert.Equal(2, context.TimeBlocks.Include(tb => tb.Repeats).Single(tb => tb.Guid == toCreate.Guid).Repeats.Count);
                Assert.Empty(context.TimeBlocks.Include(tb => tb.Exceptions).Single(tb => tb.Guid == toCreate.Guid).Exceptions);
            }
            finally
            {
                // Clearup

                using var context = GetContext();
                if (newId != 0)
                {
                    context.TimeBlocks.Remove(context.TimeBlocks.Find(newId)!);
                    await context.SaveChangesAsync();
                }
            }
        }

        [Fact]
        public async Task UpdateTimeBlock_NotFound()
        {
            var toUpdate = new TimeBlock()
            {
                Guid = Guid.NewGuid()
            };

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Update(toUpdate));
            Assert.Contains("Cannot find", ex.Message);
        }

        [Fact]
        public async Task UpdateTimeBlock()
        {
            var toUpdate = new TimeBlock();
            var existingId = -1;
            var repeatsCount = -1;

            using (var context = GetContext())
            {
                toUpdate = context.TimeBlocks.Include(tb => tb.Repeats).OrderBy(tb => tb.Id).First(tb => tb.Repeats.Count > 0);

                existingId = toUpdate.Id;
                repeatsCount = toUpdate.Repeats.Count;

                toUpdate.Name = "My Newly Updated Name";
                toUpdate.StartTime = DateTime.MaxValue.AddMinutes(-50);
                toUpdate.EndTime = DateTime.MaxValue;
                toUpdate.BusinessId = 0; // We won't have this when changing from a DTO
                toUpdate.Id = 0; // We won't have this when changing from a DTO
            }

            var result = await _repo.Update(toUpdate);

            using var db = GetContext();

            var updated = db.TimeBlocks.Single(tb => tb.Id == existingId);

            Assert.True(result);
            Assert.Equal("My Newly Updated Name", updated.Name);
            Assert.Equal(DateTime.MaxValue.AddMinutes(-50), updated.StartTime);
            Assert.Equal(DateTime.MaxValue, updated.EndTime);
            Assert.Equal(await db.GetBusinessId(), updated.BusinessId);
        }

        [Fact]
        public async Task UpdateTimeBlock_ChangeRepeatType_RemovesExistingExceptions()
        {
            var toUpdate = new TimeBlock();
            int existingId = 0;

            using (var context = GetContext())
            {
                toUpdate = context.TimeBlocks.Include(tb => tb.Repeats).Include(tb => tb.Exceptions).OrderBy(tb => tb.Id).First(tb => tb.Repeats.Count > 0);

                toUpdate.Exceptions.Add(new()
                {
                    BusinessId = await context.GetBusinessId(),
                    Name = "Ensure we have >= 1 Exception",
                    DateToReplace = DateOnly.MinValue
                });

                await context.SaveChangesAsync();

                existingId = toUpdate.Id;

                toUpdate.Name = "My Newly Updated Name # 2";
                toUpdate.StartTime = DateTime.MaxValue.AddMinutes(-150);
                toUpdate.EndTime = DateTime.MaxValue.AddMinutes(-100);
                toUpdate.RepeatType = (toUpdate.RepeatType == Entity.RepeatType.Weekly ? Entity.RepeatType.MonthlyAbsolute : Entity.RepeatType.Weekly);
                toUpdate.BusinessId = 0; // We won't have this when changing from a DTO
                toUpdate.Id = 0; // We won't have this when changing from a DTO
            }

            var result = await _repo.Update(toUpdate);

            using var db = GetContext();

            var updated = db.TimeBlocks.Include(tb => tb.Repeats).Include(tb => tb.Exceptions).Single(tb => tb.Id == existingId);

            Assert.True(result);
            Assert.Equal("My Newly Updated Name # 2", updated.Name);
            Assert.Equal(DateTime.MaxValue.AddMinutes(-150), updated.StartTime);
            Assert.Equal(DateTime.MaxValue.AddMinutes(-100), updated.EndTime);
            Assert.Equal(await db.GetBusinessId(), updated.BusinessId);
            Assert.Empty(updated.Exceptions);
        }

        [Fact]
        public async Task UpdateTimeBlock_ChangeRepeats_RemovesExistingRepeats()
        {
            var toUpdate = new TimeBlock();
            var existingId = -1;

            using (var context = GetContext())
            {
                toUpdate = context.TimeBlocks.Include(tb => tb.Repeats).Include(tb => tb.Exceptions).OrderBy(tb => tb.Id).First(tb => tb.Repeats.Count > 0);

                toUpdate.Exceptions.Add(new()
                {
                    BusinessId = await context.GetBusinessId(),
                    Name = "Ensure we have >= 1 Exception",
                    DateToReplace = DateOnly.MinValue
                });

                await context.SaveChangesAsync();

                existingId = toUpdate.Id;

                toUpdate.Name = "My Newly Updated Name # 3";
                toUpdate.StartTime = DateTime.MinValue;
                toUpdate.EndTime = DateTime.MinValue.AddMinutes(100);
                toUpdate.Repeats.Add(new() { DayIdentifier = 5 }); // Ensure we add a new repeat
                toUpdate.BusinessId = 0; // We won't have this when changing from a DTO
                toUpdate.Id = 0; // We won't have this when changing from a DTO
            }

            var result = await _repo.Update(toUpdate);

            using var db = GetContext();

            var updated = db.TimeBlocks.Include(tb => tb.Repeats).Include(tb => tb.Exceptions).Single(tb => tb.Id == existingId);

            Assert.True(result);
            Assert.Equal("My Newly Updated Name # 3", updated.Name);
            Assert.Equal(DateTime.MinValue, updated.StartTime);
            Assert.Equal(DateTime.MinValue.AddMinutes(100), updated.EndTime);
            Assert.Equal(await db.GetBusinessId(), updated.BusinessId);
            Assert.Empty(updated.Exceptions);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteTimeBlock_NotFound(bool deleteExceptions)
        {
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.DeleteTimeBlock(Guid.NewGuid(), deleteExceptions));
            Assert.Contains("Cannot find", ex.Message);
        }

        [Fact]
        public async Task DeleteTimeBlock_DeleteExceptions()
        {
            var guid = Guid.Empty;
            var id = 0;
            var exceptionId = 0;

            using (var context = GetContext())
            {
                var businessId = await context.GetBusinessId();

                var tb = new TimeBlock()
                {
                    BusinessId = businessId,
                    Name = "Something",
                    RepeatType = Entity.RepeatType.Weekly,
                    Repeats = [new() { DayIdentifier = 1 }],

                };

                context.TimeBlocks.Add(tb);

                await context.SaveChangesAsync();

                var tbEx = new TimeBlockException()
                {
                    BusinessId = businessId,
                    Name = "Excepted",
                    TimeBlockId = tb.Id
                };

                context.TimeBlockExceptions.Add(tbEx);

                await context.SaveChangesAsync();

                id = tb.Id;
                guid = tb.Guid;

                exceptionId = tbEx.Id;
            }

            var result = await _repo.DeleteTimeBlock(guid, deleteExceptions: true);

            Assert.True(result);

            using var db = GetContext();
            var timeBlocks = db.TimeBlocks.Include(tb => tb.Repeats).ToList();
            var timeBlockExceptions = db.TimeBlockExceptions.ToList();

            Assert.Null(timeBlocks.SingleOrDefault(tb => tb.Guid == guid));
            Assert.Null(timeBlockExceptions.SingleOrDefault(tbe => tbe.TimeBlockId == id || tbe.Id == exceptionId)); // Delete the exceptions
            Assert.Null(timeBlocks.SelectMany(tb => tb.Repeats).SingleOrDefault(r => r.TimeBlockId == id)); // Delete the Repeaters
        }

        [Fact]
        public async Task DeleteTimeBlock_KeepExceptions()
        {
            var guid = Guid.Empty;
            var id = 0;
            var exceptionId = 0;

            using (var context = GetContext())
            {
                var businessId = await context.GetBusinessId();

                var tb = new TimeBlock()
                {
                    BusinessId = businessId,
                    Name = "Something",
                    RepeatType = Entity.RepeatType.Weekly,
                    Repeats = [new() { DayIdentifier = 1 }],

                };

                context.TimeBlocks.Add(tb);

                await context.SaveChangesAsync();

                var tbEx = new TimeBlockException()
                {
                    BusinessId = businessId,
                    Name = "Excepted",
                    TimeBlockId = tb.Id,
                    DateToReplace = new DateOnly(2099, 12, 12)
                };

                context.TimeBlockExceptions.Add(tbEx);

                await context.SaveChangesAsync();

                id = tb.Id;
                guid = tb.Guid;

                exceptionId = tbEx.Id;
            }

            var result = await _repo.DeleteTimeBlock(guid, deleteExceptions: false);

            Assert.True(result);

            using var db = GetContext();

            var timeBlocks = db.TimeBlocks.Include(tb => tb.Repeats).ToList();
            var timeBlockExceptions = db.TimeBlockExceptions.ToList();

            Assert.Null(timeBlocks.SingleOrDefault(tb => tb.Guid == guid));
            Assert.Null(timeBlockExceptions.SingleOrDefault(tbe => tbe.TimeBlockId == id)); // Unlink the exceptions
            Assert.NotNull(timeBlockExceptions.SingleOrDefault(tbe => tbe.Id == exceptionId && tbe.TimeBlockId == null)); // Unlinked but still exists
            Assert.Null(timeBlocks.SelectMany(tb => tb.Repeats).SingleOrDefault(r => r.TimeBlockId == id)); // Delete the Repeaters
        }

        [Fact]
        public async Task CreateTimeBlockException_InvalidGuid()
        {
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Create(new TimeBlockException(), Guid.NewGuid()));
            Assert.Contains("Cannot find", ex.Message);
        }

        [Fact]
        public async Task CreateTimeBlockException_Exists()
        {
            using var context = GetContext();

            var tb = context.TimeBlocks.OrderBy(tb => tb.Id).First();
            tb.Exceptions.Add(new()
            {
                Name = "A new name",
                DateToReplace = new DateOnly(2020, 1, 1),
                BusinessId = await context.GetBusinessId()
            });

            await context.SaveChangesAsync();

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Create(new TimeBlockException() { Name = "Something", DateToReplace = new DateOnly(2020, 1, 1) }, tb.Guid));
            Assert.Contains("exists", ex.Message);
        }

        [Fact]
        public async Task CreateTimeBlockException()
        {
            TimeBlock tb = new();
            using (var context = GetContext())
            {

                tb = context.TimeBlocks.OrderBy(tb => tb.Id).First();
            }

            var tbe = new TimeBlockException()
            {
                Name = "A new name",
                DateToReplace = new DateOnly(2020, 1, 1),
            };

            var result = await _repo.Create(tbe, tb.Guid);

            using var db = GetContext();
            Assert.True(result);
            Assert.Equal(await db.GetBusinessId(), tbe.BusinessId);
            Assert.True(tbe.Id > 0);
            Assert.NotEqual(Guid.Empty, tbe.Guid);

            Assert.Contains(db.TimeBlocks.Include(x => x.Exceptions).Single(x => x.Id == tb.Id).Exceptions, e => e.Id == tbe.Id && e.Name == tbe.Name && e.DateToReplace == tbe.DateToReplace);
        }

        [Fact]
        public async Task UpdateTimeBlockException_InvalidTimeBlockGuid()
        {
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Update(new TimeBlockException(), Guid.NewGuid()));
            Assert.Contains("Cannot find", ex.Message);
            Assert.DoesNotContain("exception", ex.Message.ToLower());
        }

        [Fact]
        public async Task UpdateTimeBlockException_NoneFound()
        {
            using var context = GetContext();
            var tbGuid = context.TimeBlocks.OrderBy(tb => tb.Id).First().Guid;

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Update(new TimeBlockException(), tbGuid));
            Assert.Contains("Cannot find", ex.Message);
            Assert.Contains("exception", ex.Message.ToLower());
        }

        [Fact]
        public async Task UpdateTimeBlockException()
        {
            TimeBlock tb = new();
            TimeBlockException tbe = new();
            using (var context = GetContext())
            {

                tb = context.TimeBlocks.OrderBy(tb => tb.Id).First();
                tbe = new TimeBlockException()
                {
                    Name = "A new name",
                    DateToReplace = new DateOnly(2020, 1, 1),
                    BusinessId = await context.GetBusinessId()
                };

                tb.Exceptions.Add(tbe);
                await context.SaveChangesAsync();
            }

            var toUpdate = new TimeBlockException()
            {
                Guid = tbe.Guid,
                Name = "Updated Name",
                DateToReplace = new DateOnly(2022, 5, 5),
                StartTime = new DateTime(2024, 1, 1, 10, 15, 0),
                EndTime = new DateTime(2024, 1, 1, 11, 30, 0),
            };

            var result = await _repo.Update(toUpdate, tb.Guid);

            using var db = GetContext();
            Assert.True(result);
            Assert.Equal(await db.GetBusinessId(), toUpdate.BusinessId);
            Assert.True(toUpdate.Id > 0);
            Assert.Equal(tbe.Guid, toUpdate.Guid);

            Assert.Contains(db.TimeBlocks.Include(x => x.Exceptions).Single(x => x.Id == tb.Id).Exceptions, e => 
                            e.Id == toUpdate.Id && e.Name == toUpdate.Name && e.DateToReplace == toUpdate.DateToReplace &&
                            e.StartTime == toUpdate.StartTime && e.EndTime == toUpdate.EndTime);
        }

        [Fact]
        public async Task DeleteException_NewWithoutTimeBlock()
        {
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.DeleteException(new(), Guid.NewGuid()));
            Assert.Contains("Cannot find", ex.Message);
        }

        [Fact]
        public async Task DeleteExeption_New()
        {
            using var context = GetContext();

            var tb = context.TimeBlocks.OrderBy(tb => tb.Id).First();

            var tbe = new TimeBlockException()
            {
                Name = "Deleted",
                DateToReplace = new DateOnly(2020, 10, 20),
                StartTime = new DateTime(2020, 10, 20, 10, 10, 10),
                EndTime = new DateTime(2020, 10, 20, 12, 10, 10),
            };

            var result = await _repo.DeleteException(tbe, tb.Guid);

            Assert.True(result);
            Assert.True(tbe.Id > 0);

            var updated = context.TimeBlockExceptions.IgnoreQueryFilters().Single(x => x.Guid == tbe.Guid);

            Assert.Equal(tb.Id, updated.TimeBlockId);
            Assert.Equal(await context.GetBusinessId(), updated.BusinessId);
            Assert.Equal(updated.StartTime, updated.EndTime); // Deleted instances are set to have 0 duration. Ensure start and end are the same
        }

        [Fact]
        public async Task DeleteExeption_Existing()
        {
            using var context = GetContext();

            var tb = context.TimeBlocks.OrderBy(tb => tb.Id).First();

            var tbe = new TimeBlockException()
            {
                Name = "Deleted",
                DateToReplace = new DateOnly(2020, 10, 20),
                StartTime = new DateTime(2020, 10, 20, 10, 10, 10),
                EndTime = new DateTime(2020, 10, 20, 12, 10, 10),
                BusinessId = await context.GetBusinessId()
            };

            tb.Exceptions.Add(tbe);
            await context.SaveChangesAsync();

            tbe.Id = 0; // Wouldn't know this in the DTO form
            tbe.BusinessId = 0; // Wouldn't know this in the DTO form
            tbe.TimeBlockId = 0; // Wouldn't know this in the DTO form

            var result = await _repo.DeleteException(tbe, tb.Guid);

            Assert.True(result);

            var updated = context.TimeBlockExceptions.IgnoreQueryFilters().Single(x => x.Guid == tbe.Guid);

            Assert.True(updated.Id > 0);
            Assert.Equal(tb.Id, updated.TimeBlockId);
            Assert.Equal(await context.GetBusinessId(), updated.BusinessId);
            Assert.Equal(updated.StartTime, updated.EndTime); // Deleted instances are set to have 0 duration. Ensure start and end are the same
        }

        [Fact]
        public async Task DeleteExeption_Unlinked()
        {
            TimeBlockException tbe = new();
            int initialCount = -1;
            using (var context = GetContext()) {

                initialCount = context.TimeBlockExceptions.ToList().Count;

                tbe = new TimeBlockException()
                {
                    Name = "Unlinked",
                    DateToReplace = new DateOnly(2020, 10, 20),
                    StartTime = new DateTime(2020, 10, 20, 10, 10, 10),
                    EndTime = new DateTime(2020, 10, 20, 12, 10, 10),
                    BusinessId = await context.GetBusinessId()
                };

                context.TimeBlockExceptions.Add(tbe);
                await context.SaveChangesAsync();

                Assert.Equal(initialCount + 1, context.TimeBlockExceptions.ToList().Count);
            }

            var toDelete = new TimeBlockException()
            {
                Guid = tbe.Guid
            };

            var result = await _repo.DeleteException(toDelete, Guid.Empty);

            using var db = GetContext();
            // In this instance we should fully delete the TimeBlockException as there is no assosciated TimeBlock
            Assert.True(result);
            Assert.Equal(initialCount, db.TimeBlockExceptions.Count());
            Assert.Null(db.TimeBlockExceptions.SingleOrDefault(x => x.Guid == toDelete.Guid));
        }
    }
}
