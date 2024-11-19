using Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Data.Tests.Fixtures.BookingService
{
    public class BookingContextFixture : DockerSqlFixture, IDbContextFactory<BookingServiceDbContext>
    {

        internal BookingServiceDbContext CreateContext()
        {
            return new(
        new DbContextOptionsBuilder<BookingServiceDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options
        );
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            using var bookingContext = CreateContext();
        }

        internal override async Task SeedDatabase(ApplicationDbContext context)
        {
            await BookingSeedData.SeedDatabase(context);
        }

        BookingServiceDbContext IDbContextFactory<BookingServiceDbContext>.CreateDbContext() => CreateContext();
    }
}