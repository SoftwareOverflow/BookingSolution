using Auth.Interfaces;
using Data.Context;
using Data.Repository;
using Data.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Data.Tests
{
    public class BusinessRepoTests : BaseUserChangeTestClass
    {
        private readonly BusinessRepo _repo;
        private readonly BusinessRepo _localRepo;

        public BusinessRepoTests(DockerSqlFixture fixture) : base(fixture)
        {
            _repo = new BusinessRepo(fixture);
            _localRepo = new BusinessRepo(this);
        }

        [Fact]
        public async Task GetBusiness()
        {
            var result = await _repo.GetBusiness();

            using var context = GetContext();

            var expectedBusiness = context.BusinessUsers.Include(bu => bu.Business).IgnoreQueryFilters().Single(bu => bu.UserId == DockerSqlFixture.UserId).Business;

            // Ensure we get the correct result
            Assert.NotNull(result);
            Assert.Equal(expectedBusiness.Id, result.Id);
            Assert.Equal(expectedBusiness.Guid, result.Guid);
            Assert.Equal(expectedBusiness.Name, result.Name);

            // Ensure other businesses exist to double check we got the right one
            Assert.Contains(context.Businesses.IgnoreQueryFilters().ToList(), b => b.Id != expectedBusiness.Id);

            await Task.Delay(5000);
        }

        [Fact]
        public async Task GetBusiness_NoUserThenLogin()
        {
            MockUserId  = "";

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _localRepo.GetBusiness());

            using var context = GetContext();
            // mock the login by changing the userId to a matching value
            var changedUser = context.BusinessUsers.IgnoreQueryFilters().OrderByDescending(bu => bu.UserId).Last();
            MockUserId = changedUser.UserId;

            var result = await _localRepo.GetBusiness();
            Assert.Equal(changedUser.BusinessId, result!.Id);
        }

        [Fact]
        public async Task GetBusiness_UserNoBusiness()
        {
            MockUserId = Guid.NewGuid().ToString();

            var result = await _localRepo.GetBusiness();
            
            // We don't want to throw and error here, as when the user is first created they won't have registered their business yet.
            Assert.Null(result);
        }

        [Fact]
        public async Task RegisterBusiness()
        {
            MockUserId = Guid.NewGuid().ToString();

            // Business to create
            var businessToRegister = SeedData.GetBusinesses(1).Single();
            businessToRegister.Users = [];
            businessToRegister.Services = [];
            businessToRegister.Appointments = [];

            var result = await _localRepo.RegisterBusiness(businessToRegister);

            Assert.True(result);

            using var contex = GetContext();
            var created = contex.Businesses.IgnoreQueryFilters().OrderBy(x => x.Id).Include(b => b.Address).Include(b => b.Users).Last();

            Assert.NotEqual(Guid.Empty, created.Guid);
            Assert.Equal(MockUserId, created.Users.Single().UserId);
            Assert.Equivalent(businessToRegister.Address, created.Address);
            Assert.Equal(businessToRegister.AddressId, created.AddressId);
        }

        [Fact]
        public async Task RegisterBusiness_NoUser()
        {
            MockUserId = "";

            // Business to create
            var businessToRegister = SeedData.GetBusinesses(1).Single();
            businessToRegister.Users = [];
            businessToRegister.Services = [];
            businessToRegister.Appointments = [];


            var repoToTest = new BusinessRepo(this);
            await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.RegisterBusiness(businessToRegister));
        }

        [Fact]
        public async Task RegisterBusiness_UserHasBusiness()
        {
            // Business to create
            var businessToRegister = SeedData.GetBusinesses(1).Single();
            businessToRegister.Users = [];
            businessToRegister.Services = [];
            businessToRegister.Appointments = [];

            await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.RegisterBusiness(businessToRegister));
        }
    }
}
