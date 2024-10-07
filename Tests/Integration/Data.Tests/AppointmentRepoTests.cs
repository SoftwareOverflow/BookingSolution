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
                Assert.Equal(initialCount +1, apts.Count);
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
    }
}
