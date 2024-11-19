using Auth.Interfaces;
using Data.Context;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.MsSql;

namespace Data.Tests.Fixtures
{
    public class DockerSqlFixture : IAsyncLifetime, IDbContextFactory<ApplicationDbContext>
    {

        protected MsSqlContainer _container;

        private Mock<IUserService> _userServiceMock = new();
        public static readonly string UserId = Guid.NewGuid().ToString();

        private static readonly object _lock = new();
        private static bool _databaseInitialized;

        public DockerSqlFixture()
        {
            _container = new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2022-latest").WithCleanUp(true).Build();

            _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);

            //lock (_lock)
            //{
            //    if (!_databaseInitialized)
            //    {
            //        using (var context = CreateContext())
            //        {
            //            context.Database.EnsureDeleted();
            //            context.Database.EnsureCreated();

            //            // TODO add mock data
            //        }

            //        _databaseInitialized = true;
            //    }
            //}
        }


        internal ApplicationDbContext CreateContext(Mock<IUserService>? mock = null) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options,
            mock?.Object ?? _userServiceMock.Object
        );

        public virtual async Task InitializeAsync()
        {
            try
            {
                await _container.StartAsync();

                using var context = CreateContext();
                await context.Database.MigrateAsync();
                ClearDatabase(context);
                await SeedDatabase(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        internal virtual async Task SeedDatabase(ApplicationDbContext context)
        {
            await SeedData.SeedDatabase(context);
        }

        public Task DisposeAsync()
        {

            return _container.StopAsync();
        }

        private async void ClearDatabase(ApplicationDbContext context)
        {
            context.Businesses.RemoveRange(context.Businesses.IgnoreQueryFilters().ToList());
            context.BusinessUsers.RemoveRange(context.BusinessUsers.IgnoreQueryFilters().ToList());
            context.Appointments.RemoveRange(context.Appointments.IgnoreQueryFilters().ToList());
            context.People.RemoveRange(context.People.IgnoreQueryFilters().ToList());
            context.Services.RemoveRange(context.Services.IgnoreQueryFilters().ToList());

            await context.SaveChangesAsync();
        }

        ApplicationDbContext IDbContextFactory<ApplicationDbContext>.CreateDbContext() => CreateContext();
    }
}
