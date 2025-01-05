using Data.Entity;
using Data.Entity.Appointments;
using Data.Interfaces;
using Data.Repository.Anon;
using Data.Tests.Fixtures.BookingService;
using Microsoft.EntityFrameworkCore;

namespace Data.Tests
{
    public class BookingRepoTests : IClassFixture<BookingContextFixture>
    {
        private readonly BookingContextFixture _fixture;
        private readonly IBookingRepo _repo;

        public BookingRepoTests(BookingContextFixture fixture)
        {
            _fixture = fixture;
            _repo = new BookingRepo(_fixture);
        }

        [Fact]
        public async Task GetServicesForBusiness_InvalidGuid()
        {
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.GetServicesForBusiness(Guid.NewGuid()));
            Assert.Contains("Cannot find", ex.Message);
        }

        [Fact]
        public async Task GetServicesForBusiness_Empty()
        {
            using var context = _fixture.CreateContext();

            var business = context.Businesses.OrderBy(b => b.Guid).Include(b => b.Services).First(b => b.Guid != BookingSeedData.ValidBusinessGuid);
            context.Appointments.RemoveRange(context.Appointments.Where(a => a.BusinessId == business.Id));
            context.Services.RemoveRange(business.Services);
            await context.SaveChangesAsync();

            var result = await _repo.GetServicesForBusiness(business.Guid);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetServicesForBusiness()
        {
            var services = new List<Service>
            {
                new()
                {
                    Name = "Service # 1",
                    EarliestTime = new TimeOnly(9, 0, 0),
                    LatestTime = new TimeOnly(16, 0, 0),
                    RepeatType = Entity.RepeatType.Weekly,
                    Repeats = [new() { DayIdentifier = (int)DayOfWeek.Monday }],
                    StartDate = DateOnly.MinValue,
                    Price = 19.9900m
                },
                new()
                {
                    Name = "Service # 2",
                    EarliestTime = new TimeOnly(11, 0, 0),
                    LatestTime = new TimeOnly(15, 0, 0),
                    RepeatType = Entity.RepeatType.Weekly,
                    Repeats = [new(){ DayIdentifier = (int) DayOfWeek.Wednesday }],
                    StartDate = new DateOnly(2024, 1, 1),
                    BookingFrequencyMins = 15,
                    DurationMins = 60,
                    Price = 12.9900m
                }
            };

            using (var context = _fixture.CreateContext())
            {
                var business = context.Businesses.Include(b => b.Services).Single(b => b.Guid == BookingSeedData.ValidBusinessGuid);

                //context.Services.RemoveRange(business.Services);
                //await context.SaveChangesAsync();

                business.Services = services;
                await context.SaveChangesAsync();
            }

            var result = await _repo.GetServicesForBusiness(BookingSeedData.ValidBusinessGuid);

            Assert.Equal(2, result.Count);
            foreach (var r in result)
            {
                Assert.Contains(services, s => s.Guid == r.Guid &&
                                               s.EarliestTime == r.EarliestTime &&
                                               s.LatestTime == r.LatestTime &&
                                               s.StartDate == r.StartDate &&
                                               s.BookingFrequencyMins == r.BookingFrequencyMins &&
                                               s.Price == r.Price &&
                                               s.DurationMins == r.DurationMins);
            }
        }

        [Fact]
        public async Task GetService_InvalidBusinessGuid()
        {
            using var context = _fixture.CreateContext();
            var business = context.Businesses.Include(b => b.Services).Single(b => b.Guid == BookingSeedData.ValidBusinessGuid);

            var service = business.Services.OrderBy(s => s.Guid).First();

            var result = await _repo.GetService(Guid.NewGuid(), service.Guid);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetService_InvalidServiceGuid()
        {
            var result = await _repo.GetService(BookingSeedData.ValidBusinessGuid, Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetService()
        {
            using var context = _fixture.CreateContext();
            var business = context.Businesses.Include(b => b.Services).Single(b => b.Guid == BookingSeedData.ValidBusinessGuid);

            var service = business.Services.OrderBy(s => s.Guid).First();

            var result = await _repo.GetService(business.Guid, service.Guid);

            Assert.NotNull(result);
            Assert.Equal(service.Guid, result.Guid);
            Assert.Equal(service.Id, result.Id);
            Assert.Equal(service.Name, result.Name);
        }

        [Fact]
        public async Task GetTimeBlocksForBusiness_InvalidGuid()
        {
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.GetTimeBlocksForBusiness(Guid.NewGuid()));
            Assert.Contains("Cannot find", ex.Message);
        }

        [Fact]
        public async Task GetTimeBlocksForBusiness_Empty()
        {
            using var context = _fixture.CreateContext();

            var business = context.Businesses.Single(b => b.Guid == BookingSeedData.ValidBusinessGuid);
            // Ensure the business has no time blocks
            context.TimeBlocks.RemoveRange(context.TimeBlocks.Where(tb => tb.BusinessId == business.Id));


            var otherBusiness = context.Businesses.OrderBy(b => b.Id).First(b => b.Id != business.Id);

            // Ensure other businesses have time blocks
            otherBusiness.TimeBlocks.Add(new Entity.Appointments.TimeBlock()
            {
                Name = "Something"
            });

            await context.SaveChangesAsync();

            var result = await _repo.GetTimeBlocksForBusiness(business.Guid);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimeBlocksForBusiness()
        {
            using var context = _fixture.CreateContext();

            var business = context.Businesses.Single(b => b.Guid == BookingSeedData.ValidBusinessGuid);
            // Ensure the business has time blocks
            context.TimeBlocks.RemoveRange(context.TimeBlocks.Where(tb => tb.BusinessId == business.Id));
            await context.SaveChangesAsync();

            context.TimeBlocks.Add(new Entity.Appointments.TimeBlock()
            {
                Name = "TB #1",
                BusinessId = business.Id,
                Repeats = [new() {
                    DayIdentifier = 5,
                    Index = -1
                }],
                StartTime = new DateTime(2023, 10, 10, 10, 30, 0),
                EndTime = new DateTime(2023, 10, 10, 11, 45, 0),
            });
            context.TimeBlocks.Add(new Entity.Appointments.TimeBlock()
            {
                Name = "TB #2",
                BusinessId = business.Id,
                Repeats = [new(), new()],
                Exceptions = [new() { Name = "Exception for # 2", BusinessId = business.Id }]
            });
            context.TimeBlocks.Add(new Entity.Appointments.TimeBlock()
            {
                Name = "TB #3",
                BusinessId = business.Id,
                Repeats = [new(), new(), new()],
                Exceptions = [new() { Name = "Exception for # 3", BusinessId = business.Id }, new() { Name = "Another exception for # 3", BusinessId = business.Id }]
            });

            await context.SaveChangesAsync();

            var otherBusiness = context.Businesses.OrderBy(b => b.Id).First(b => b.Id != business.Id);

            // Ensure other businesses have time blocks
            context.TimeBlocks.Add(new Entity.Appointments.TimeBlock()
            {
                Name = "Something",
                BusinessId = otherBusiness.Id
            });

            await context.SaveChangesAsync();

            var result = await _repo.GetTimeBlocksForBusiness(BookingSeedData.ValidBusinessGuid);

            Assert.Equal(3, result.Count);

            var tb1 = context.TimeBlocks.Single(tb => tb.Name == "TB #1");
            Assert.Single(tb1.Repeats);
            Assert.Empty(tb1.Exceptions);

            var tb2 = context.TimeBlocks.Single(tb => tb.Name == "TB #2");
            Assert.Equal(2, tb2.Repeats.Count);
            Assert.Single(tb2.Exceptions);

            var tb3 = context.TimeBlocks.Single(tb => tb.Name == "TB #3");
            Assert.Equal(3, tb3.Repeats.Count);
            Assert.Equal(2, tb3.Exceptions.Count);
        }

        [Fact]
        public async Task GetTimeBlockExceptionsBetweenDates_InvalidGuid()
        {
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.GetTimeBlockExceptionsBetweenDates(Guid.NewGuid(), DateOnly.FromDayNumber(100), DateOnly.FromDayNumber(105)));
            Assert.Contains("Cannot find", ex.Message);
        }

        [Fact]
        public async Task GetTimeBlockExceptionsBetweenDates_Empty()
        {
            using var context = _fixture.CreateContext();

            var business = context.Businesses.Single(b => b.Guid == BookingSeedData.ValidBusinessGuid);
            // Ensure the business has no time block exceptions
            context.TimeBlockExceptions.RemoveRange(context.TimeBlockExceptions.Where(tb => tb.BusinessId == business.Id));


            var otherBusiness = context.Businesses.OrderBy(b => b.Id).First(b => b.Id != business.Id);

            // Ensure other businesses have time blocks
            otherBusiness.TimeBlocks.Add(new Entity.Appointments.TimeBlock()
            {
                Name = "Something",
                Exceptions = [
                    new(){
                        Name = "Exception",
                        BusinessId = otherBusiness.Id
                    }
                    ]
            });

            await context.SaveChangesAsync();

            var result = await _repo.GetTimeBlockExceptionsBetweenDates(BookingSeedData.ValidBusinessGuid, DateOnly.MinValue, DateOnly.MaxValue);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimeBlockExceptionsBetweenDates()
        {
            using var context = _fixture.CreateContext();

            var business = context.Businesses.Single(b => b.Guid == BookingSeedData.ValidBusinessGuid);
            context.TimeBlockExceptions.RemoveRange(context.TimeBlockExceptions.Where(tb => tb.BusinessId == business.Id));


            context.TimeBlocks.AddRange([
                new(){
                    BusinessId = business.Id,
                    Name = "TB 1",
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddMinutes(15),
                    RepeatType = RepeatType.Weekly,
                    Repeats = [
                        new() {
                            DayIdentifier = 1, Index = -1
                        }
                        ],
                    Exceptions = [
                        // Straddles end, but should be returned
                        new(){
                            BusinessId = business.Id,
                            Name = "0",
                            DateToReplace = DateOnly.MaxValue,
                            StartTime = new DateTime(2020, 1, 3, 10, 0, 0),
                            EndTime = new DateTime(2020, 1, 29, 10, 0, 0),
                        },
                        // Straddles start, should be returned.
                        new() {
                            BusinessId = business.Id,
                            Name = "1",
                            DateToReplace = DateOnly.MinValue,
                            StartTime = new DateTime(2020, 1, 1, 10, 0, 0),
                            EndTime = new DateTime(2020, 1, 5, 10, 15, 0),
                        },
                        // Wholly inside, should be returned
                        new() {
                            BusinessId = business.Id,
                            Name = "2",
                            DateToReplace = DateOnly.MinValue,
                            StartTime = new DateTime(2020, 1, 10, 9, 30, 0),
                            EndTime = new DateTime(2020, 1, 10, 11, 15, 0)
                        },
                        // Inside, but 0 duration - should NOT be returned
                        new() {
                            BusinessId = business.Id,
                            Name = "N/A",
                            StartTime = new DateTime(2020, 1, 10, 9, 30, 0),
                            EndTime = new DateTime(2020, 1, 10, 9, 30, 0)
                        },
                        // Wholly before, should NOT be returned
                        new() {
                            BusinessId = business.Id,
                            Name = "N/A",
                            DateToReplace = DateOnly.MinValue,
                            StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                            EndTime = new DateTime(2000, 1, 1, 17, 0, 0),
                        },
                        // Wholly after, should NOT be returned
                        new() {
                            BusinessId = business.Id,
                            Name = "N/A",
                            DateToReplace = DateOnly.MinValue,
                            StartTime = new DateTime(2500, 1, 1, 0, 0, 0),
                            EndTime = new DateTime(2500, 1, 1, 17, 0, 0),
                        }
                        ]
                }
                ]);

            context.TimeBlockExceptions.AddRange([
                new(){
                    TimeBlockId = null,
                    BusinessId = business.Id,
                    Name = "3",
                    StartTime = new DateTime(2020, 1, 10, 10, 0, 0),
                    EndTime = new DateTime(2020, 1, 10, 11, 15, 0),
                },
                // Outside, should not be returned
                new(){
                    TimeBlockId = null,
                    BusinessId = business.Id,
                    Name = "N/A",
                    StartTime = new DateTime(2010, 1, 10, 10, 0, 0),
                    EndTime = new DateTime(2010, 1, 10, 11, 15, 0),
                },
                // Inside but empty, should not returned
                new(){
                    TimeBlockId = null,
                    BusinessId = business.Id,
                    Name = "N/A",
                    StartTime = new DateTime(2020, 1, 10, 10, 0, 0),
                    EndTime = new DateTime(2020, 1, 10, 10, 0, 0),
                },

                ]);
            var otherBusiness = context.Businesses.OrderBy(b => b.Id).First(b => b.Id != business.Id);

            // Ensure other businesses have time blocks
            otherBusiness.TimeBlocks.Add(new Entity.Appointments.TimeBlock()
            {
                Name = "N/A",
                Exceptions = [
                    new(){
                        Name = "N/A",
                        BusinessId = otherBusiness.Id
                    }
                    ]
            });

            await context.SaveChangesAsync();

            var result = await _repo.GetTimeBlockExceptionsBetweenDates(BookingSeedData.ValidBusinessGuid, new DateOnly(2020, 1, 2), new DateOnly(2020, 1, 20));

            Assert.Equal(4, result.Count);

            for (int i = 0; i < result.Count; i++)
            {
                Assert.Contains(result, r => r.Name == i.ToString());
            }

            Assert.DoesNotContain(result, r => r.Name == "N/A");
        }

        [Fact]
        public async Task GetBookingsBetweenDates_InvalidGuid()
        {
            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.GetBookingsBetweenDates(Guid.NewGuid(), DateOnly.FromDayNumber(100), DateOnly.FromDayNumber(105)));
            Assert.Contains("Cannot find", ex.Message);
        }

        [Fact]
        public async Task GetBookingsBetweenDates_Empty()
        {
            using var context = _fixture.CreateContext();

            var business = context.Businesses.Single(b => b.Guid == BookingSeedData.ValidBusinessGuid);
            // Ensure the business has no time block exceptions
            context.Appointments.RemoveRange(context.Appointments.Where(tb => tb.BusinessId == business.Id));


            var otherBusiness = context.Businesses.OrderBy(b => b.Id).First(b => b.Id != business.Id);

            // Ensure other businesses have time blocks
            otherBusiness.Appointments.Add(new()
            {
                Name = "Something",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddDays(1),
                Person = new()
                {
                    FirstName = "A",
                    LastName = "B",
                    EmailAddress = "A@B.com",
                    PhoneNumber = "0 123 456 789",
                }
            });

            await context.SaveChangesAsync();

            var result = await _repo.GetBookingsBetweenDates(BookingSeedData.ValidBusinessGuid, DateOnly.MinValue, DateOnly.MaxValue);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBookingsBetweenDates()
        {
            using var context = _fixture.CreateContext();

            var business = context.Businesses.Single(b => b.Guid == BookingSeedData.ValidBusinessGuid);
            context.Appointments.RemoveRange(context.Appointments.Where(tb => tb.BusinessId == business.Id));


            context.Appointments.AddRange([
                // Straddles end, but should be returned
                new(){
                    BusinessId = business.Id,
                    Name = "0",
                    StartTime = new DateTime(2020, 1, 3, 10, 0, 0),
                    EndTime = new DateTime(2020, 1, 29, 10, 0, 0),
                    Person = new(){
                        FirstName = "A",
                        LastName = "B",
                        EmailAddress = "A@B.com",
                        PhoneNumber="0 123 456 789",
                    }
                },
                // Straddles start, should be returned.
                new() {
                    BusinessId = business.Id,
                    Name = "1",
                    StartTime = new DateTime(2020, 1, 1, 10, 0, 0),
                    EndTime = new DateTime(2020, 1, 5, 10, 15, 0),
                    Person = new(){
                        FirstName = "A",
                        LastName = "B",
                        EmailAddress = "A@B.com",
                        PhoneNumber="0 123 456 789",
                    }
                },
                // Wholly inside, should be returned
                new() {
                    BusinessId = business.Id,
                    Name = "2",
                    StartTime = new DateTime(2020, 1, 10, 9, 30, 0),
                    EndTime = new DateTime(2020, 1, 10, 11, 15, 0),
                    Person = new(){
                        FirstName = "A",
                        LastName = "B",
                        EmailAddress = "A@B.com",
                        PhoneNumber="0 123 456 789",
                    }
                },
                // Inside, but 0 duration - should NOT be returned
                new() {
                    BusinessId = business.Id,
                    Name = "N/A",
                    StartTime = new DateTime(2020, 1, 10, 9, 30, 0),
                    EndTime = new DateTime(2020, 1, 10, 9, 30, 0),
                    Person = new(){
                        FirstName = "A",
                        LastName = "B",
                        EmailAddress = "A@B.com",
                        PhoneNumber="0 123 456 789",
                    }
                },
                // Wholly before, should NOT be returned
                new() {
                    BusinessId = business.Id,
                    Name = "N/A",
                    StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                    EndTime = new DateTime(2000, 1, 1, 17, 0, 0),
                    Person = new(){
                        FirstName = "A",
                        LastName = "B",
                        EmailAddress = "A@B.com",
                        PhoneNumber="0 123 456 789",
                    }
                },
                // Wholly after, should NOT be returned
                new() {
                    BusinessId = business.Id,
                    Name = "N/A",
                    StartTime = new DateTime(2500, 1, 1, 0, 0, 0),
                    EndTime = new DateTime(2500, 1, 1, 17, 0, 0),
                    Person = new(){
                        FirstName = "Aa",
                        LastName = "Bb",
                        EmailAddress = "Ab@Ba.co.uk",
                        PhoneNumber="+44 12378781278",
                    }
                }
                ]);

            var otherBusiness = context.Businesses.OrderBy(b => b.Id).First(b => b.Id != business.Id);

            // Ensure other businesses have time blocks
            otherBusiness.Appointments.Add(new ()
            {
                Name = "N/A",
                Person = new()
                {
                    FirstName = "Aaa",
                    LastName = "Bbbb",
                    EmailAddress = "Aaaa@Bbb.com",
                    PhoneNumber = "0987654321",
                }
            });

            await context.SaveChangesAsync();

            var result = await _repo.GetBookingsBetweenDates(BookingSeedData.ValidBusinessGuid, new DateOnly(2020, 1, 2), new DateOnly(2020, 1, 20));

            Assert.Equal(3, result.Count);

            for (int i = 0; i < result.Count; i++)
            {
                Assert.Contains(result, r => r.Name == i.ToString());
            }

            Assert.DoesNotContain(result, r => r.Name == "N/A");
        }

        [Fact]
        public async Task CreateBookingRequest_NoPerson()
        {
            var apt = new Appointment()
            {
                BookingType = BookingType.Online,
                Name = "New Booking"
            };

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.CreateBookingRequest(apt, BookingSeedData.ValidBusinessGuid));
            Assert.Contains("Person", ex.Message);
        }

        [Fact]
        public async Task CreateBookingRequest_InvalidBusinessGuid()
        {
            using var context = _fixture.CreateContext();

            var business = context.Businesses.Include(b => b.Services).Single(b => b.Guid == BookingSeedData.ValidBusinessGuid);

            var service = business.Services.OrderBy(s => s.Guid).First();
            // Remove IDs we wouldn't know when creating from DTO.
            service.Id = 0;
            service.BusinessId = 0;

            var apt = new Appointment()
            {
                BookingType = BookingType.Online,
                Name = "New Booking",
                Person = new()
                {
                    FirstName = "Mock",
                    LastName = "Person",
                    EmailAddress = "mock.person@mockperson.com",
                    PhoneNumber = "0128189212"
                },
                Service = service
            };

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.CreateBookingRequest(apt, Guid.NewGuid()));
            Assert.Contains("Cannot find", ex.Message);
        }

        [Fact]
        public async Task CreateBookingRequest_InvalidServiceGuid()
        {
            using var context = _fixture.CreateContext();

            var apt = new Appointment()
            {
                BookingType = BookingType.Online,
                Name = "New Booking",
                Person = new()
                {
                    FirstName = "Mock",
                    LastName = "Person",
                    EmailAddress = "mock.person@mockperson.com",
                    PhoneNumber = "0128189212"
                },
                Service = new()
                {
                    Guid = Guid.NewGuid()
                }
            };

            var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _repo.CreateBookingRequest(apt,BookingSeedData.ValidBusinessGuid));
            Assert.Contains("Cannot find", ex.Message);
        }

        [Fact]
        public async Task CreateBookingRequest()
        {
            using var context = _fixture.CreateContext();

            var business = context.Businesses.Include(b => b.Services).Single(b => b.Guid == BookingSeedData.ValidBusinessGuid);

            var service = business.Services.OrderBy(s => s.Guid).First();
            // Remove IDs we wouldn't know when creating from DTO.
            service.Id = 0;
            service.BusinessId = 0;

            var apt = new Appointment()
            {
                BookingType = BookingType.Online,
                Name = "New Booking - TEST",
                Person = new()
                {
                    FirstName = "Mock",
                    LastName = "Person",
                    EmailAddress = "mock.person@mockperson.com",
                    PhoneNumber = "0128189212"
                },
                Service = service
            };

            var result = await _repo.CreateBookingRequest(apt, BookingSeedData.ValidBusinessGuid);

            var saved = context.Appointments.Single(a => a.Name == "New Booking - TEST");

            Assert.True(result);
            Assert.Equal(BookingState.Pending, saved.State); // Ensure we create booking requests in the pending state
        }
    }
}
