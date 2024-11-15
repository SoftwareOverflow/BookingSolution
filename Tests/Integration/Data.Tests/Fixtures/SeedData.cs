using Bogus;
using Data.Context;
using Data.Entity;
using Data.Entity.Appointments;
using NuGet.Packaging;

namespace Data.Tests.Fixtures
{
    internal class SeedData
    {

        private static readonly int _seed = Random.Shared.Next(10, int.MaxValue);

        // TODO think about seeding,and what data we might need access to within the tests!

        public static async Task SeedDatabase(ApplicationDbContext context)
        {
            // Business & BusinessUsers
            var businessses = GetBusinesses(Random.Shared.Next(25));
            businessses.Add(GetBusinesses(1, true).Single());
            businessses.AddRange(GetBusinesses(Random.Shared.Next(7)));

            await context.Businesses.AddRangeAsync(businessses);
            await context.SaveChangesAsync();

            // Appointments
            var minBusinessId = businessses.MinBy(x => x.Id)!.Id;
            var maxBusinessId = businessses.MaxBy(x => x.Id)!.Id;

            var businessIdForMockUser = businessses.Single(x => x.Users.SingleOrDefault(u => u.UserId == DockerSqlFixture.UserId) != null).Id;

            ICollection<Appointment> appointments = [];
            ICollection<TimeBlock> timeBlocks = [];
            foreach (var business in businessses)
            {
                int minAppointments = 0;
                int maxAppointments = 10;

                List<int?> serviceIds = [null];
                serviceIds.AddRange(business.Services.Select(s => (int?)s.Id));


                appointments.AddRange(GetAppointments(Random.Shared.Next(minAppointments, maxAppointments), business.Id, false, serviceIds[Random.Shared.Next(serviceIds.Count)]));

                if (business.Id == businessIdForMockUser)
                {
                    minAppointments = 3;
                    appointments.Add(GetAppointments(1, business.Id, true, null).Single()); // Ensure we have at least one appointment without a service
                    serviceIds.Remove(null); // Ensure we have appointments linked to services

                    timeBlocks.AddRange(GetTimeBlocks(10, business.Id));
                }
                else
                {
                    timeBlocks.AddRange(GetTimeBlocks(Random.Shared.Next(5), business.Id));
                }

                appointments.AddRange(GetAppointments(Random.Shared.Next(minAppointments, maxAppointments), business.Id, true, serviceIds[Random.Shared.Next(serviceIds.Count)]));
            }

            await context.Appointments.AddRangeAsync(appointments);
            await context.TimeBlocks.AddRangeAsync(timeBlocks);


            await context.SaveChangesAsync();
        }


        public static List<Business> GetBusinesses(int count, bool ownedByMockUser = false) => GetBusinessFaker(ownedByMockUser).Generate(count);

        private static Faker<Business> GetBusinessFaker(bool ownedByMockUser) => new Faker<Business>()
            .RuleFor(b => b.Id, o => 0)
            .RuleFor(b => b.Address, o => GetAddress())
            .RuleFor(b => b.Name, (faker, t) => faker.Company.CompanyName())
            .RuleFor(b => b.Url, (faker, t) => faker.Internet.Url())
            .RuleFor(b => b.Users, (faker, t) => GetBusinessUserFaker(ownedByMockUser).Generate(1))
            .RuleFor(b => b.Services, (faker, t) => GetServices(ownedByMockUser ? 2 : 0 + Random.Shared.Next(5)))
            .UseSeed(_seed);

        private static Address GetAddress() => GetAddressFaker().Generate(1).Single();

        private static Faker<Address> GetAddressFaker() => new Faker<Address>()
            .RuleFor(a => a.Address1, (faker, t) => Random.Shared.Next(5) == 1 ? string.Join(" ", faker.Lorem.Words(Random.Shared.Next(1, 3))) : faker.Address.BuildingNumber())
            .RuleFor(a => a.Address2, (faker, t) => Random.Shared.Next(5) == 1 ? faker.Address.SecondaryAddress() : "")
            .RuleFor(a => a.Address3, (faker, t) => faker.Address.StreetName())
            .RuleFor(a => a.City, (faker, t) => faker.Address.City())
            .RuleFor(a => a.State, (faker, t) => Random.Shared.Next(5) == 1 ? faker.Address.State() : "")
            .UseSeed(_seed);

        private static Faker<BusinessUser> GetBusinessUserFaker(bool ownedByMockUser) => new Faker<BusinessUser>()
            .RuleFor(b => b.UserId, o => ownedByMockUser ? DockerSqlFixture.UserId : Guid.NewGuid().ToString())
            .UseSeed(_seed);

        public static List<Service> GetServices(int count) => GetServiceFaker().Generate(count);

        private static Faker<Service> GetServiceFaker()
        {
            int[] bookingFreqMins = [15, 30, 60];
            return new Faker<Service>()
            .RuleFor(s => s.Id, o => 0)
            .RuleFor(s => s.Name, (faker, t) => faker.Name.JobType())
            .RuleFor(s => s.BookingFrequencyMins, o => o.PickRandom(bookingFreqMins))
            .RuleFor(s => s.Price, o => o.Finance.Amount(0, 100, 4))
            .RuleFor(s => s.EarliestTime, o => new TimeOnly(Random.Shared.Next(24), Random.Shared.Next(3) * 15))
            .RuleFor(s => s.LatestTime, (faker, s) => faker.Date.BetweenTimeOnly(s.EarliestTime, new TimeOnly(23, 59)))
            .RuleFor(s => s.StartDate, o => o.Date.BetweenDateOnly(DateOnly.FromDateTime(DateTime.Now).AddMonths(-6), DateOnly.FromDateTime(DateTime.Now).AddMonths(6)))
            .RuleFor(s => s.RepeatType, o => o.PickRandom<RepeatType>())
            .RuleFor(s => s.Repeats, (faker, t) => GetRepeats(t.RepeatType, Random.Shared.Next(6) + 1))
            .UseSeed(_seed);
        }

        public static List<Appointment> GetAppointments(int count, int businessId, bool future = false, int? serviceId = null) => GetAppointmentFaker(businessId, future, serviceId).Generate(count);

        private static Faker<Appointment> GetAppointmentFaker(int businessId, bool future, int? serviceId)
        {
            return new Faker<Appointment>()
                .RuleFor(a => a.BusinessId, o => businessId)
                .RuleFor(a => a.ServiceId, o => serviceId)
                .RuleFor(a => a.Name, (faker, t) => faker.Name.JobTitle())
                .RuleFor(a => a.StartTime, (faker, t) => future ? faker.Date.Future() : faker.Date.Past())
                .RuleFor(a => a.EndTime, (faker, t) => t.StartTime.AddTicks(Random.Shared.Next(1000) * 60000))
                .RuleFor(a => a.BookingType, (faker, t) => faker.PickRandom<BookingType>())
                .RuleFor(a => a.Person, (faker, t) => GetPerson())
                .UseSeed(_seed);
        }

        public static List<TimeBlock> GetTimeBlocks(int count, int businessId) => GetTimeBlockFaker(businessId).Generate(count);

        private static Faker<TimeBlock> GetTimeBlockFaker(int businessId)
        {
            return new Faker<TimeBlock>()
                .RuleFor(tb => tb.Name, o => o.Name.JobType())
                .RuleFor(tb => tb.BusinessId, businessId)
                .RuleFor(a => a.StartTime, (faker, t) => faker.Date.Past())
                .RuleFor(a => a.EndTime, (faker, t) => t.StartTime.AddTicks(Random.Shared.Next(1000) * 60000))
                .RuleFor(s => s.RepeatType, o => o.PickRandom<RepeatType>())
                .RuleFor(s => s.Repeats, (faker, t) => GetTimeBlockRepeats(t.RepeatType!.Value, Random.Shared.Next(6) + 1))
                .UseSeed(_seed);
        }

        private static List<TimeBlockRepeater> GetTimeBlockRepeats(RepeatType repeatType, int count)
        {
            var picks = new Dictionary<int, List<int>>();

            if (repeatType == RepeatType.Weekly)
            {
                picks.Add(0, Enumerable.Range(0, 6).ToList());
            }
            else if (repeatType == RepeatType.MonthlyAbsolute)
            {
                picks.Add(0, Enumerable.Range(0, 31).ToList());
            }
            else if (repeatType == RepeatType.MonthlyRelative)
            {
                picks.Add(-1, Enumerable.Range(0, 6).ToList());
                picks.Add(1, Enumerable.Range(0, 6).ToList());
                picks.Add(2, Enumerable.Range(0, 6).ToList());
                picks.Add(3, Enumerable.Range(0, 6).ToList());
            }

            var result = new List<TimeBlockRepeater>();

            for (int i = 0; i < count; i++)
            {
                var repeater = GetTimeBlockRepeaterFaker(repeatType, picks).Generate(1).Single();

                picks[repeater.Index ?? 0].Remove(repeater.DayIdentifier);
                if (picks[repeater.Index ?? 0].Count == 0)
                {
                    picks.Remove(repeater.Index ?? 0);
                }

                result.Add(repeater);

                if (picks.Keys.Count == 0)
                {
                    break;
                }
            }

            return result;
        }

        private static Faker<TimeBlockRepeater> GetTimeBlockRepeaterFaker(RepeatType type, Dictionary<int, List<int>> picks)
        {
            if (type == RepeatType.Weekly)
            {
                return new Faker<TimeBlockRepeater>()
                   .RuleFor(r => r.DayIdentifier, o => o.PickRandom(picks[0]))
                   .UseSeed(_seed);
            }
            else if (type == RepeatType.MonthlyAbsolute)
            {
                return new Faker<TimeBlockRepeater>()
                    .RuleFor(r => r.Index, o => o.PickRandom<int>(picks.Keys))
                    .RuleFor(r => r.DayIdentifier, (faker, t) => faker.PickRandom(picks[0]))
                    .UseSeed(_seed);
            }
            else if (type == RepeatType.MonthlyRelative)
            {
                return new Faker<TimeBlockRepeater>()
                   .RuleFor(r => r.Index, o => o.PickRandom<int>(picks.Keys))
                   .RuleFor(r => r.DayIdentifier, (faker, t) => faker.PickRandom(picks[t.Index!.Value]))
                   .UseSeed(_seed);
            }

            throw new NotImplementedException();
        }

        private static Entity.Appointments.Person GetPerson() => GetPeopleFaker().Generate(1).Single();

        private static Faker<Entity.Appointments.Person> GetPeopleFaker() => new Faker<Entity.Appointments.Person>()
            .RuleFor(p => p.FirstName, (faker, p) => faker.Name.FirstName())
            .RuleFor(p => p.LastName, (faker, p) => faker.Name.LastName())
            .RuleFor(p => p.PhoneNumber, (faker, p) => faker.Phone.PhoneNumber())
            .RuleFor(p => p.EmailAddress, (faker, p) => faker.Internet.Email())
            .UseSeed(_seed);

        private static List<ServiceRepeater> GetRepeats(RepeatType repeatType, int count)
        {
            var picks = new Dictionary<int, List<int>>();

            if (repeatType == RepeatType.Weekly)
            {
                picks.Add(0, Enumerable.Range(0, 6).ToList());
            }
            else if (repeatType == RepeatType.MonthlyAbsolute)
            {
                picks.Add(0, Enumerable.Range(0, 31).ToList());
            }
            else if (repeatType == RepeatType.MonthlyRelative)
            {
                picks.Add(-1, Enumerable.Range(0, 6).ToList());
                picks.Add(1, Enumerable.Range(0, 6).ToList());
                picks.Add(2, Enumerable.Range(0, 6).ToList());
                picks.Add(3, Enumerable.Range(0, 6).ToList());
            }

            var result = new List<ServiceRepeater>();

            for (int i = 0; i < count; i++)
            {
                var repeater = GetServiceRepeaterFaker(repeatType, picks).Generate(1).Single();

                picks[repeater.Index ?? 0].Remove(repeater.DayIdentifier);
                if (picks[repeater.Index ?? 0].Count == 0)
                {
                    picks.Remove(repeater.Index ?? 0);
                }

                result.Add(repeater);

                if (picks.Keys.Count == 0)
                {
                    break;
                }
            }

            return result;
        }

        private static Faker<ServiceRepeater> GetServiceRepeaterFaker(RepeatType type, Dictionary<int, List<int>> picks)
        {
            if (type == RepeatType.Weekly)
            {
                return new Faker<ServiceRepeater>()
                   .RuleFor(r => r.DayIdentifier, o => o.PickRandom(picks[0]))
                   .UseSeed(_seed);
            }
            else if (type == RepeatType.MonthlyAbsolute)
            {
                return new Faker<ServiceRepeater>()
                    .RuleFor(r => r.Index, o => o.PickRandom<int>(picks.Keys))
                    .RuleFor(r => r.DayIdentifier, (faker, t) => faker.PickRandom(picks[0]))
                    .UseSeed(_seed);
            }
            else if (type == RepeatType.MonthlyRelative)
            {
                return new Faker<ServiceRepeater>()
                   .RuleFor(r => r.Index, o => o.PickRandom<int>(picks.Keys))
                   .RuleFor(r => r.DayIdentifier, (faker, t) => faker.PickRandom(picks[t.Index!.Value]))
                   .UseSeed(_seed);
            }

            throw new NotImplementedException();
        }
    }
}
