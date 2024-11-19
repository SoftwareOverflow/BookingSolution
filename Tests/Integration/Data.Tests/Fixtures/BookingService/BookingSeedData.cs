using Data.Context;
using Data.Entity.Appointments;
using NuGet.Packaging;

namespace Data.Tests.Fixtures.BookingService
{
    internal class BookingSeedData
    {
        public static Guid ValidBusinessGuid = Guid.Empty;

        public static async Task SeedDatabase(ApplicationDbContext context)
        {
            // Business
            var businessses = SeedData.GetBusinesses(Random.Shared.Next(25));
            businessses.Add(SeedData.GetBusinesses(1, true).Single());
            businessses.AddRange(SeedData.GetBusinesses(Random.Shared.Next(7)));

            await context.Businesses.AddRangeAsync(businessses);
            await context.SaveChangesAsync();

            // Appointments
            var minBusinessId = businessses.MinBy(x => x.Id)!.Id;
            var maxBusinessId = businessses.MaxBy(x => x.Id)!.Id;

            ValidBusinessGuid = context.Businesses.Single(b => b.Id == minBusinessId).Guid; // The first business won't have any appointments so I can set up tests safely.

            ICollection<Appointment> appointments = [];
            foreach (var business in businessses)
            {
                int minAppointments = 0;
                int maxAppointments = 10;

                List<int?> serviceIds = [null];
                serviceIds.AddRange(business.Services.Select(s => (int?)s.Id));

                if (business.Id != minBusinessId)
                {
                    minAppointments = 3;
                    appointments.Add(SeedData.GetAppointments(1, business.Id, true, null).Single()); // Ensure we have at least one appointment without a service
                    serviceIds.Remove(null); // Ensure we have appointments linked to services

                    appointments.AddRange(SeedData.GetAppointments(Random.Shared.Next(minAppointments, maxAppointments), business.Id, true, serviceIds[Random.Shared.Next(serviceIds.Count)]));
                }
            }

            await context.Appointments.AddRangeAsync(appointments);

            await context.SaveChangesAsync();
        }
    }
}
