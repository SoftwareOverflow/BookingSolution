using Data.Entity;
using Data.Entity.Appointments;
using Data.Repository;
using Data.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Data.Tests
{
    public class ServiceRepoTests : BaseUserChangeTestClass
    {
        private readonly ServiceRepo _repo;
        private readonly ServiceRepo _localRepo;

        public ServiceRepoTests(DockerSqlFixture factory) : base(factory)
        {
            _repo = new ServiceRepo(factory);
            _localRepo = new ServiceRepo(this);
        }

        [Fact]
        public async Task GetServices()
        {
            var result = await _repo.GetServices();

            using var context = GetContext();

            // Check all returned services are for the users business
            var expectedBusinessId = context.BusinessUsers.IgnoreQueryFilters().Single(bu => bu.UserId == DockerSqlFixture.UserId).BusinessId;
            Assert.All(result, s => Assert.Equal(expectedBusinessId, s.BusinessId));

            // Check that other services actually exist, ensuring we received the correct subset
            var allServices = context.Services.IgnoreQueryFilters().ToList();

            // Ensure all other services do NOT contain the expected businessId
            Assert.True(result.Count() < allServices.Count());
            allServices.RemoveAll(s => result.Any(r => r.Guid == s.Guid));
            Assert.All(allServices, s => Assert.NotEqual(expectedBusinessId, s.BusinessId));
        }

        [Fact]
        public async Task GetServices_NoUser()
        {
            MockUserId = "";

            await Assert.ThrowsAnyAsync<Exception>(async () => await _localRepo.GetServices());
        }

        [Fact]
        public async Task GetServices_UserNoBusiness()
        {
            MockUserId = Guid.NewGuid().ToString();

            var result = await _localRepo.GetServices();
            Assert.Empty(result);
        }

        [Fact]
        public async Task Create()
        {
            var serviceToCreate = SeedData.GetServices(1).Single();

            int initialServices = 0;
            using (var context = GetContext())
            {

                var business = context.BusinessUsers.IgnoreQueryFilters().Include(bu => bu.Business).ThenInclude(b => b.Services).Single(bu => bu.UserId == DockerSqlFixture.UserId).Business;
                initialServices = business.Services.Count;
            }

            var result = await _repo.Create(serviceToCreate);

            Assert.True(result);


            using (var context = GetContext())
            {
                var business = context.BusinessUsers.IgnoreQueryFilters().Include(bu => bu.Business).ThenInclude(b => b.Services).ThenInclude(s => s.Repeats).Single(bu => bu.UserId == DockerSqlFixture.UserId).Business;
                var servies = business.Services;
                var newService = servies.OrderBy(s => s.Id).Last();

                Assert.Equal(initialServices + 1, servies.Count);

                // Don't care about navigation properties
                newService.Business = null;

                Assert.Equivalent(serviceToCreate, newService);
            }
        }

        [Fact]
        public async Task Create_AlreadyExists()
        {
            var serviceToCreate = SeedData.GetServices(1).Single();

            using (var context = GetContext())
            {
                var business = context.BusinessUsers.IgnoreQueryFilters().Include(bu => bu.Business).ThenInclude(b => b.Services).Single(bu => bu.UserId == DockerSqlFixture.UserId).Business;
                serviceToCreate.Guid = business.Services.ToList().First().Guid;
            }

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Create(serviceToCreate));
            Assert.Contains("already exists", ex.Message);
            Assert.Contains("Call Update", ex.Message);
        }


        [Fact]
        public async Task Create_NoUser()
        {
            MockUserId = "";
            var repoToTest = new ServiceRepo(this);

            var serviceToCreate = SeedData.GetServices(1).Single();
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await repoToTest.Create(serviceToCreate));
            Assert.Contains("user", ex.Message.ToLower());
        }

        [Fact]
        public async Task Create_UserNoBusiness()
        {
            MockUserId = Guid.NewGuid().ToString();
            var repoToTest = new ServiceRepo(this);

            var serviceToCreate = SeedData.GetServices(1).Single();
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await repoToTest.Create(serviceToCreate));
            Assert.Contains("business", ex.Message.ToLower());
        }

        [Fact]
        public async Task Update()
        {
            var service = new Service()
            {
                BookingFrequencyMins = 30,
                DurationMins = 60,
                EarliestTime = new TimeOnly(9, 0),
                LatestTime = new TimeOnly(17, 30),
                Name = "Original Service Name",
                Price = 29.9900m, // Price scale saves as 4dp in db
                RepeatType = ServiceRepeatType.Weekly,
                Repeats = [new() { DayIdentifier = 1}, new() { DayIdentifier = 3}],
                StartDate = new DateOnly(2024, 09, 20)
            };

            var result = await _repo.Create(service);

            Assert.True(result);

            service.BookingFrequencyMins = 15;
            service.DurationMins = 45;
            service.EarliestTime = new TimeOnly(10, 15);
            service.LatestTime = new TimeOnly(18, 0);
            service.Name = "Update Service Name";
            service.Price = 14.4900m; // Price scale saves as 4dp in db
            service.RepeatType = ServiceRepeatType.MonthlyAbsolute;
            service.Repeats = [new() { DayIdentifier = 2 }, new() { DayIdentifier = 5 }, new() { DayIdentifier = 10 }];

            result = await _repo.Update(service);

            Assert.True(result);

            using var context = GetContext();
            var saved = await context.Services.Include(s => s.Repeats).SingleAsync(s => s.Guid == service.Guid);

            Assert.Equivalent(saved, service);
        }

        [Fact]
        public async Task Update_NoUser()
        {
            MockUserId = "";

            var result = await _repo.GetServices();
            var service = result.OrderBy(s => s.Id).First();
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _localRepo.Update(service));
            Assert.Contains("user", ex.Message.ToLower());
        }

        [Fact]
        public async Task Update_UserNoBusiness()
        {
            MockUserId = Guid.NewGuid().ToString();

            var result = await _repo.GetServices();
            var service = result.OrderBy(s => s.Id).First();
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _localRepo.Update(service));
            Assert.Contains("user", ex.Message.ToLower());
        }

        [Fact]
        public async Task Update_NoService()
        {
            var service = SeedData.GetServices(1).Single();
            service.Guid = Guid.NewGuid(); // Ensure we can't match it to an existing service

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Update(service));
            Assert.Contains("service", ex.Message.ToLower());
        }

        [Fact]
        public async Task Delete()
        {
            var guidToDelete = Guid.Empty;
            int initialServices = 0;
            List<Appointment> linkedAppointments = [];
            using (var context = GetContext())
            {
                var business = context.BusinessUsers.IgnoreQueryFilters().Include(bu => bu.Business).ThenInclude(b => b.Services).ThenInclude(s => s.Appointments).Single(bu => bu.UserId == DockerSqlFixture.UserId).Business;
                initialServices = business.Services.Count;

                var toDelete = business.Services.OrderBy(x => x.Guid).First(s => s.Appointments.Count > 0);
                linkedAppointments = toDelete.Appointments.ToList();
                guidToDelete =  toDelete.Guid;
            }

            var result = await _repo.Delete(guidToDelete);

            Assert.True(result);

            using (var context = GetContext())
            {
                var business = context.BusinessUsers.IgnoreQueryFilters().Include(bu => bu.Business).ThenInclude(b => b.Services).ThenInclude(s => s.Repeats).Single(bu => bu.UserId == DockerSqlFixture.UserId).Business;
                var servies = business.Services;

                var apts = context.Appointments.ToList();

                Assert.Equal(initialServices - 1, servies.Count);
                Assert.DoesNotContain(servies, s => s.Guid == guidToDelete);

                // Ensure we did NOT delete the assosciated appointments, instead unlinking them from the service
                Assert.All(linkedAppointments, la => apts.Single(a => a.Id == la.Id && a.ServiceId == null));
            }
        }

        [Fact]
        public async Task Delete_UserNoBusiness()
        {
            MockUserId = Guid.NewGuid().ToString();

            var result = await _repo.GetServices();
            var service = result.OrderBy(s => s.Id).First();
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _localRepo.Delete(service.Guid));
            Assert.Contains("service", ex.Message.ToLower());
        }

        [Fact]
        public async Task Delete_NoService()
        {
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.Delete(Guid.NewGuid()));
            Assert.Contains("service", ex.Message.ToLower());
        }
    }
}
