using AutoMapper;
using Core.Dto;
using Core.Dto.BookingRequest;
using Core.Dto.Services;
using Core.Interfaces;
using Core.Mapping;
using Core.Responses;
using Core.Services;
using Data.Entity;
using Data.Entity.Appointments;
using Data.Interfaces;
using Moq;

namespace Core.Tests.Services
{
    public class BookingServiceTests
    {

        private readonly Mock<IBookingContext> BookingContext = new();
        private readonly IMapper Mapper;

        private readonly BookingService BookingService;

        public BookingServiceTests()
        {
            var mappingConfig = new MapperConfiguration(x => x.AddProfile(new AutoMapperConfig()));
            Mapper = mappingConfig.CreateMapper();

            BookingService = new BookingService(BookingContext.Object, Mapper);
        }

        [Fact]
        public async void GetBookingRequest_NoService()
        {
            var businessGuid = Guid.NewGuid();
            var serviceGuid = Guid.NewGuid();

            BookingContext.Setup(x => x.GetService(businessGuid, serviceGuid)).Returns(Task.FromResult<Service?>(null));

            var result = await BookingService.GetNewBookingRequest(businessGuid, serviceGuid);

            Assert.Equal(ResultType.ClientError, result.ResultType);
            Assert.Null(result.Result);
            Assert.Contains(result.Errors, x => x.Contains("business"));

            BookingContext.Verify(x => x.GetService(businessGuid, serviceGuid), Times.Once);
        }

        [Fact]
        public async void GetBookingRequest_Returns()
        {
            var businessGuid = Guid.NewGuid();
            var serviceGuid = Guid.NewGuid();
            var service = new Service()
            {
                Guid = serviceGuid,
                BusinessId = 5,
                DurationMins = 120,
                EarliestTime = new TimeOnly(8, 30),
                LatestTime = new TimeOnly(18, 00),
            };

            BookingContext.Setup(x => x.GetService(businessGuid, serviceGuid)).Returns(Task.FromResult<Service?>(service));

            var result = await BookingService.GetNewBookingRequest(businessGuid, serviceGuid);

            var serviceDto = Mapper.Map<ServiceTypeDto>(service);

            var bookingRequest = new BookingRequestDto(serviceDto, businessGuid, new PersonDto(), DateOnly.FromDateTime(DateTime.Now));

            Assert.Equal(ResultType.Success, result.ResultType);
            Assert.Equal(serviceGuid, result.Result!.Service.Guid);
            Assert.Equivalent(bookingRequest, result.Result);

            BookingContext.Verify(x => x.GetService(businessGuid, serviceGuid), Times.Once);
        }

        [Fact]
        public async void GetBookingRequest_DatabaseThrows_Fails()
        {
            BookingContext.Setup(x => x.GetService(It.IsAny<Guid>(), It.IsAny<Guid>())).Throws(new Exception("Internal database error"));

            var result = await BookingService.GetNewBookingRequest(Guid.NewGuid(), Guid.NewGuid());

            Assert.Equivalent(ServiceResult<ServiceTypeDto>.DefaultServerFailure(), result);
        }

        [Fact]
        public void NextServiceDate_NoRepeats_ServerError()
        {
            var service = new ServiceTypeDto()
            {
                RepeatType = ServiceRepeaterTypeDto.MonthlyAbsolute,
                Repeats = []
            };

            var result = BookingService.GetNextServiceDate(service, new DateOnly(2024, 9, 1));

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ServerError, result.ResultType);
            Assert.Contains(result.Errors, x => x.Contains("dates"));
        }

        [Fact]
        public void NextServiceDate_DateBeforeStart_ClientError()
        {
            var service = new ServiceTypeDto()
            {
                RepeatType = ServiceRepeaterTypeDto.MonthlyAbsolute,
                Repeats = [new(17)],
                StartDate = new DateTime(2024, 9, 2)
            };

            var result = BookingService.GetNextServiceDate(service, new DateOnly(2024, 9, 1));

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ClientError, result.ResultType);
            Assert.Contains(result.Errors, x => x.Contains("start"));
        }

        [Fact]
        public void NextServiceDate_WeeklyRepeats_MatchDate()
        {
            var date = new DateOnly(2024, 8, 28); // Wednesday

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.Weekly,
                Repeats = [new(DayOfWeek.Monday), new(DayOfWeek.Wednesday)]
            };

            var result = BookingService.GetNextServiceDate(service, date);

            Assert.True(result.IsSuccess);
            Assert.Equal(date, result.Result);
        }

        [Fact]
        public void NextServiceDate_WeeklyRepeats()
        {
            var date = new DateOnly(2024, 8, 28); // Wednesday

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.Weekly,
                Repeats = [new(DayOfWeek.Monday), new(DayOfWeek.Sunday)]
            };

            var result = BookingService.GetNextServiceDate(service, date);

            var expectedDate = new DateOnly(2024, 9, 1); // First Sunday after date

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDate, result.Result);
        }

        [Fact]
        public void NextServiceDate_MonthlyAbsoluteRepeats_MatchDate()
        {
            var date = new DateOnly(2024, 8, 8);

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.MonthlyAbsolute,
                Repeats = [new(1), new(8), new(17)]
            };

            var result = BookingService.GetNextServiceDate(service, date);

            Assert.True(result.IsSuccess);
            Assert.Equal(date, result.Result);
        }

        [Fact]
        public void NextServiceDate_MonthlyAbsoluteRepeats_AllBefore()
        {
            var date = new DateOnly(2024, 8, 28);

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.MonthlyAbsolute,
                Repeats = [new(19), new(15), new(23), new(25)]
            };

            var result = BookingService.GetNextServiceDate(service, date);

            var expectedDate = new DateOnly(2024, 9, 15);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDate, result.Result);
        }

        [Fact]
        public void NextServiceDate_MonthlyAbsolute()
        {
            var date = new DateOnly(2024, 8, 20);

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.MonthlyAbsolute,
                Repeats = [new(10), new(19), new(27)]
            };

            var result = BookingService.GetNextServiceDate(service, date);

            var expectedDate = new DateOnly(2024, 8, 27);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDate, result.Result);
        }

        [Fact]
        public void NextServiceDate_MonthlyAbsolute_AllAfter()
        {
            var date = new DateOnly(2024, 8, 10);

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.MonthlyAbsolute,
                Repeats = [new(18), new(19), new(11), new(27)]
            };

            var result = BookingService.GetNextServiceDate(service, date);

            var expectedDate = new DateOnly(2024, 8, 11);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDate, result.Result);
        }

        [Fact]
        public void NextServiceDate_MonthlyRelative_Week1_MatchDate()
        {
            var date = new DateOnly(2024, 8, 5); // Monday

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.MonthlyRelative,
                Repeats = [new(DayOfWeek.Monday, 1), new(DayOfWeek.Wednesday, -1)]
            };

            var result = BookingService.GetNextServiceDate(service, date);

            Assert.True(result.IsSuccess);
            Assert.Equal(date, result.Result);
        }

        [Fact]
        public void NextServiceDate_MonthlyRelative_Week2_MatchDate()
        {
            var date = new DateOnly(2024, 8, 12); // Monday

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.MonthlyRelative,
                Repeats = [new(DayOfWeek.Monday, 2), new(DayOfWeek.Wednesday, -1)]
            };

            var result = BookingService.GetNextServiceDate(service, date);

            Assert.True(result.IsSuccess);
            Assert.Equal(date, result.Result);
        }

        [Fact]
        public void NextServiceDate_MonthlyRelative_Week3_MatchDate()
        {
            var date = new DateOnly(2024, 8, 20); // Tuesday

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.MonthlyRelative,
                Repeats = [new(DayOfWeek.Tuesday, 3), new(DayOfWeek.Wednesday, -1)]
            };

            var result = BookingService.GetNextServiceDate(service, date);

            Assert.True(result.IsSuccess);
            Assert.Equal(date, result.Result);
        }

        [Fact]
        public void NextServiceDate_MonthlyRelative_LastWeek_MatchDate()
        {
            var date = new DateOnly(2024, 8, 28); // Last Wednesday of month

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.MonthlyRelative,
                Repeats = [new(DayOfWeek.Monday, 1), new(DayOfWeek.Wednesday, -1)]
            };

            var result = BookingService.GetNextServiceDate(service, date);

            Assert.True(result.IsSuccess);
            Assert.Equal(date, result.Result);
        }

        [Fact]
        public void NextServiceDate_MonthlyRelative_Week1_SameMonth()
        {
            var date = new DateOnly(2024, 8, 1); // Thursday

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.MonthlyRelative,
                Repeats = [new(DayOfWeek.Monday, 1), new(DayOfWeek.Wednesday, -1), new(DayOfWeek.Friday, 1)]
            };

            var result = BookingService.GetNextServiceDate(service, date);

            Assert.True(result.IsSuccess);
            Assert.Equal(date.AddDays(1), result.Result);
        }

        [Fact]
        public void NextServiceDate_MonthlyRelative_Week2_SameMonth()
        {
            var date = new DateOnly(2024, 8, 1); // Thursday

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.MonthlyRelative,
                Repeats = [new(DayOfWeek.Monday, 2), new(DayOfWeek.Wednesday, -1), new(DayOfWeek.Friday, 2)]
            };

            var result = BookingService.GetNextServiceDate(service, date);
            var expectedDate = new DateOnly(2024, 8, 9); // Second Monday of the month

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDate, result.Result);
        }

        [Fact]
        public void NextServiceDate_MonthlyRelative_Week3_SameMonth()
        {
            var date = new DateOnly(2024, 8, 1); // Thursday

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.MonthlyRelative,
                Repeats = [new(DayOfWeek.Tuesday, 3)]
            };

            var result = BookingService.GetNextServiceDate(service, date);
            var expectedDate = new DateOnly(2024, 8, 20); // Third Tuesday

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDate, result.Result);
        }

        [Fact]
        public async void GetAvailability_BeforeServiceStart()
        {
            var startDate = new DateOnly(2024, 6, 20);
            var endDate = new DateOnly(2024, 6, 30);

            var service = new ServiceTypeDto()
            {
                StartDate = endDate.AddDays(7).ToDateTime(new TimeOnly()),
            };

            var result = await BookingService.GetAvailabilityBetweenDates(service, Guid.NewGuid(), startDate, endDate);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Result!.Availability);

            BookingContext.Verify(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()), Times.Never);
        }

        [Fact]
        public async void GetAvailability_EndBeforeStart()
        {
            var startDate = new DateOnly(2024, 6, 20);
            var endDate = startDate.AddDays(-3);

            var service = new ServiceTypeDto()
            {
                StartDate = startDate.AddMonths(-7).ToDateTime(new TimeOnly()),
            };

            var result = await BookingService.GetAvailabilityBetweenDates(service, Guid.NewGuid(), startDate, endDate);
            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ClientError, result.ResultType);
            Assert.Contains(result.Errors, x => x.Contains("end date"));
        }

        [Fact]
        public async void GetAvailability_AroundServiceStart()
        {
            var startDate = new DateOnly(2024, 6, 20);
            var endDate = startDate.AddDays(10);

            BookingContext.Setup(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), startDate, endDate)).Returns(Task.FromResult<ICollection<Appointment>>([]));

            var service = new ServiceTypeDto()
            {
                StartDate = startDate.AddDays(5).ToDateTime(new TimeOnly()),
                RepeatType = ServiceRepeaterTypeDto.Weekly,
                Repeats = [new(DayOfWeek.Monday)]
            };

            var result = await BookingService.GetAvailabilityBetweenDates(service, Guid.NewGuid(), startDate, endDate);

            Assert.True(result.IsSuccess);
            Assert.Equal(11, result.Result!.Availability.Count);
            Assert.All(result.Result.Availability, x => Assert.Empty(x.Times));
        }

        [Fact]
        public async void GetAvailability_WeeklyRepeats()
        {
            var startDate = new DateOnly(2024, 8, 31); // Saturday
            var endDate = new DateOnly(2024, 9, 3); // Tuesday

            BookingContext.Setup(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), startDate, endDate)).Returns(Task.FromResult<ICollection<Appointment>>([]));

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.Now.AddMonths(-1),
                EarliestTime = new TimeSpan(9, 0, 0),
                LatestTime = new TimeSpan(17, 0, 0),
                DurationMins = 120,
                BookingFrequencyMins = 60,
                RepeatType = Dto.Services.ServiceRepeaterTypeDto.Weekly,
                Repeats = [new ServiceRepeaterDto(DayOfWeek.Monday), new ServiceRepeaterDto(DayOfWeek.Tuesday), new ServiceRepeaterDto(DayOfWeek.Friday)]
            };

            var result = await BookingService.GetAvailabilityBetweenDates(service, Guid.NewGuid(), startDate, endDate);
            var dates = result.Result;

            Assert.True(result.IsSuccess);
            Assert.Equal(4, dates!.Availability.Count);

            // Repeat service every Mon, Tue and Fri. Requested Sat -> Tues
            Assert.All(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 8, 31)).Times, x => Assert.Equal(AvailabilityState.NotAvailable, x.State));
            Assert.All(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 1)).Times, x => Assert.Equal(AvailabilityState.NotAvailable, x.State));

            Assert.All(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 2)).Times, x => Assert.Equal(AvailabilityState.Available, x.State));
            Assert.All(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 3)).Times, x => Assert.Equal(AvailabilityState.Available, x.State));

            var expectedTimes = new List<TimeOnly>([
                new TimeOnly(9, 0, 0),
                new TimeOnly(10, 0, 0),
                new TimeOnly(11, 0, 0),
                new TimeOnly(12, 0, 0),
                new TimeOnly(13, 0, 0),
                new TimeOnly(14, 0, 0),
                new TimeOnly(15, 0, 0),
                ]);

            Assert.Empty(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 8, 31)).Times);
            Assert.Empty(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 1)).Times);

            Assert.Equal(expectedTimes.Count, dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 2)).Times.Count);
            Assert.Equal(expectedTimes.Count, dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 3)).Times.Count);

            expectedTimes.ForEach(time =>
            {
                Assert.Contains(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 2)).Times, x => x.Time == time);
                Assert.Contains(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 3)).Times, x => x.Time == time);
            });

            BookingContext.Verify(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), startDate, endDate), Times.Once);

        }

        [Fact]
        public async void GetAvailability_MonthlyAbsoluteRepeats()
        {
            var startDate = new DateOnly(2024, 6, 1);
            var endDate = new DateOnly(2024, 7, 1);

            BookingContext.Setup(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), startDate, endDate)).Returns(
                Task.FromResult<ICollection<Appointment>>([
                    new Appointment() // Clashes
                    {
                        Name = "ABC",
                        Person = new Person(),
                        StartTime = new DateTime(2024, 6, 3, 10, 30, 0),
                        EndTime = new DateTime(2024, 6, 3, 13, 30, 0),
                    },

                    new Appointment() // No clash
                    {
                        Name = "ABC",
                        Person = new Person(),
                        StartTime = new DateTime(2024, 6, 4, 15, 00, 0),
                        EndTime = new DateTime(2024, 6, 4, 17, 00, 0),
                    },

                    new Appointment() // Clashes
                    {
                        Name = "ABC",
                        Person = new Person(),
                        StartTime = new DateTime(2024, 6, 5, 10, 30, 0),
                        EndTime = new DateTime(2024, 6, 5, 13, 30, 0),
                    },
                    new Appointment() // Clashes
                    {
                        Name = "ABC",
                        Person = new Person(),
                        StartTime = new DateTime(2024, 6, 5, 15, 0, 0),
                        EndTime = new DateTime(2024, 6, 5, 17, 0, 0),
                    },

                    new Appointment() // No clash
                    {
                         Name = "ABC",
                        Person = new Person(),
                        StartTime = new DateTime(2024, 6, 18, 9, 00, 0),
                        EndTime = new DateTime(2024, 6, 19, 17, 00, 0),
                    },
            ]));

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.MonthlyAbsolute,
                Repeats = [new(3), new(5), new(7), new(25)],
                DurationMins = 60,
                BookingFrequencyMins = 60,
                EarliestTime = new TimeSpan(9, 0, 0),
                LatestTime = new TimeSpan(17, 0, 0)
            };

            var result = await BookingService.GetAvailabilityBetweenDates(service, Guid.NewGuid(), startDate, endDate);

            var expectedDates = new List<DateOnly>() { new(2024, 6, 3), new(2024, 6, 5), new(2024, 6, 7), new(2024, 6, 25), };
            var expectedTimes = new List<TimeOnly>() { new(9, 0), new(10, 0), new(11, 0), new(12, 0), new(13, 0), new(14, 0), new(15, 0), new(16, 0) };

            Assert.True(result.IsSuccess);
            Assert.Equivalent(expectedDates, result.Result!.Availability.Select(x => x.Date));

            // Check number of clashing days
            Assert.Equal(2, result.Result!.Availability.Where(x => x.Times.Any(y => y.State != AvailabilityState.Available)).Count());

            // Check number of clashing slots - 4 on day 1, 6 on day 2 => 10 slots
            Assert.Equal(10, result.Result!.Availability.Sum(x => x.Times.Count(y => y.State == AvailabilityState.AlreadyBooked)));

            var date1 = result.Result!.Availability.Single(x => x.Date == expectedDates[0]);
            Assert.Equal(4, date1.Times.Where(x => x.State == AvailabilityState.AlreadyBooked).Count());
            Assert.All(date1.Times.Where(x => x.Time >= new TimeOnly(10, 30) && x.Time <= new TimeOnly(13, 0)), x => Assert.Equal(AvailabilityState.AlreadyBooked, x.State));

            var date2 = result.Result!.Availability.Single(x => x.Date == expectedDates[1]);
            Assert.Equal(6, date2.Times.Where(x => x.State == AvailabilityState.AlreadyBooked).Count());
            Assert.All(date2.Times.Where(x => x.Time >= new TimeOnly(10, 30) && x.Time <= new TimeOnly(13, 0)), x => Assert.Equal(AvailabilityState.AlreadyBooked, x.State));
            Assert.All(date2.Times.Where(x => x.Time >= new TimeOnly(15, 0) && x.Time <= new TimeOnly(17, 0)), x => Assert.Equal(AvailabilityState.AlreadyBooked, x.State));
        }

        [Fact]
        public async void GetAvailability_MonthlyRelativeRepeats()
        {
            var startDate = new DateOnly(2024, 6, 1);
            var endDate = new DateOnly(2024, 6, 30);

            BookingContext.Setup(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), startDate, endDate)).Returns(
                Task.FromResult<ICollection<Appointment>>([
                    new Appointment() // Clashes
                    {
                        Name = "ABC",
                        Person = new Person(),
                        StartTime = new DateTime(2024, 6, 3, 10, 30, 0), // First Monday
                        EndTime = new DateTime(2024, 6, 3, 13, 30, 0), // First Monday
                    },

                    new Appointment() // Clash
                    {
                        Name = "ABC",
                        Person = new Person(),
                        StartTime = new DateTime(2024, 6, 28, 14, 00, 0), // Last Friday
                        EndTime = new DateTime(2024, 6, 28, 17, 00, 0), // Last Friday
                    },

                    new Appointment() // No clash
                    {
                        Name = "ABC",
                        Person = new Person(),
                        StartTime = new DateTime(2024, 6, 5, 15, 0, 0),
                        EndTime = new DateTime(2024, 6, 5, 17, 0, 0),
                    },

                    new Appointment() // Clash
                    {
                        Name = "ABC",
                        Person = new Person(),
                        StartTime = new DateTime(2024, 6, 18, 9, 00, 0), //Third Tuesday - No Clash
                        EndTime = new DateTime(2024, 6, 19, 17, 00, 0), // Third Wednesday - CLASH
                    },
            ]));

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.MinValue,
                RepeatType = ServiceRepeaterTypeDto.MonthlyRelative,
                Repeats = [new(DayOfWeek.Monday, 1), new(DayOfWeek.Wednesday, 3), new(DayOfWeek.Thursday, -1), new(DayOfWeek.Friday, -1)],
                DurationMins = 30,
                BookingFrequencyMins = 45,
                EarliestTime = new TimeSpan(12, 0, 0),
                LatestTime = new TimeSpan(15, 0, 0)
            };

            var res = await BookingService.GetAvailabilityBetweenDates(service, Guid.NewGuid(), startDate, endDate);
            var result = res.Result!;

            var expectedDates = new List<DateOnly>() { new(2024, 6, 3), new(2024, 6, 19), new(2024, 6, 27), new(2024, 6, 28), };
            var expectedTimes = new List<TimeOnly>() { new(12, 0), new(12, 45), new(13, 30), new(14, 15) };

            Assert.True(res.IsSuccess);
            Assert.Equivalent(expectedDates, result.Availability.Select(x => x.Date));

            // Check number of clashing days
            Assert.Equal(3, result.Availability.Where(x => x.Times.Any(y => y.State != AvailabilityState.Available)).Count());

            // Check number of clashing slots - 2 on day 1, 4 on day 2, 1 on day 3 => 7 slots
            Assert.Equal(7, result.Availability.Sum(x => x.Times.Count(y => y.State == AvailabilityState.AlreadyBooked)));

            var date1 = result.Availability.Single(x => x.Date == expectedDates[0]);
            Assert.Equal(2, date1.Times.Where(x => x.State == AvailabilityState.AlreadyBooked).Count());
            Assert.All(date1.Times.Where(x => x.Time >= new TimeOnly(10, 30) && x.Time <= new TimeOnly(13, 0)), x => Assert.Equal(AvailabilityState.AlreadyBooked, x.State));

            // date2 has an event all day, so ALL time slots should be set to AlreadyBooked.
            var date2 = result.Availability.Single(x => x.Date == expectedDates[1]);
            Assert.Equal(4, date2.Times.Where(x => x.State == AvailabilityState.AlreadyBooked).Count());
            Assert.All(date2.Times, x => Assert.Equal(AvailabilityState.AlreadyBooked, x.State));

            // Clash #3 is on the 4th available date, hence index 3.
            var date3 = result.Availability.Single(x => x.Date == expectedDates[3]);
            Assert.Single(date3.Times.Where(x => x.State == AvailabilityState.AlreadyBooked));
            Assert.All(date3.Times.Where(x => x.Time > new TimeOnly(13, 30) && x.Time < new TimeOnly(17, 0)), x => Assert.Equal(AvailabilityState.AlreadyBooked, x.State));
        }

        [Fact]
        public async void GetAvailability_NoClashes()
        {
            var startDate = new DateOnly(2024, 8, 31); // Saturday
            var endDate = new DateOnly(2024, 9, 3); // Tuesday

            BookingContext.Setup(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), startDate, endDate)).Returns(Task.FromResult<ICollection<Appointment>>([]));

            var service = new ServiceTypeDto()
            {
                StartDate = DateTime.Now.AddMonths(-1),
                EarliestTime = new TimeSpan(9, 0, 0),
                LatestTime = new TimeSpan(17, 0, 0),
                DurationMins = 120,
                BookingFrequencyMins = 60,
                RepeatType = Dto.Services.ServiceRepeaterTypeDto.Weekly,
                Repeats = [new ServiceRepeaterDto(DayOfWeek.Monday), new ServiceRepeaterDto(DayOfWeek.Tuesday), new ServiceRepeaterDto(DayOfWeek.Friday)]
            };

            var result = await BookingService.GetAvailabilityBetweenDates(service, Guid.NewGuid(), startDate, endDate);
            var dates = result.Result;

            Assert.True(result.IsSuccess);
            Assert.Equal(4, dates!.Availability.Count);

            // Repeat service every Mon, Tue and Fri. Requested Sat -> Tues
            Assert.All(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 8, 31)).Times, x => Assert.Equal(AvailabilityState.NotAvailable, x.State));
            Assert.All(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 1)).Times, x => Assert.Equal(AvailabilityState.NotAvailable, x.State));

            Assert.All(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 2)).Times, x => Assert.Equal(AvailabilityState.Available, x.State));
            Assert.All(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 3)).Times, x => Assert.Equal(AvailabilityState.Available, x.State));

            var expectedTimes = new List<TimeOnly>([
                new TimeOnly(9, 0, 0),
                new TimeOnly(10, 0, 0),
                new TimeOnly(11, 0, 0),
                new TimeOnly(12, 0, 0),
                new TimeOnly(13, 0, 0),
                new TimeOnly(14, 0, 0),
                new TimeOnly(15, 0, 0),
                ]);

            Assert.Empty(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 8, 31)).Times);
            Assert.Empty(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 1)).Times);

            Assert.Equal(expectedTimes.Count, dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 2)).Times.Count);
            Assert.Equal(expectedTimes.Count, dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 3)).Times.Count);

            expectedTimes.ForEach(time =>
            {
                Assert.Contains(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 2)).Times, x => x.Time == time);
                Assert.Contains(dates!.Availability.Single(x => x.Date == new DateOnly(2024, 9, 3)).Times, x => x.Time == time);
            });

            BookingContext.Verify(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), startDate, endDate), Times.Once);
        }

        [Fact]
        public async void GetAvailability_DatabaseFails()
        {
            var startDate = new DateOnly(2024, 6, 1);
            var endDate = new DateOnly(2024, 6, 30);

            BookingContext.Setup(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).Throws(new Exception("Something went wrong!"));

            var service = new ServiceTypeDto()
            {
                StartDate = startDate.ToDateTime(new TimeOnly()),
                RepeatType = ServiceRepeaterTypeDto.Weekly,
                Repeats = [new(DayOfWeek.Monday)]
            };

            var result = await BookingService.GetAvailabilityBetweenDates(service, Guid.NewGuid(), startDate, endDate);

            BookingContext.Verify(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()), Times.Once);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
        }

        [Fact]
        public async void SendBookingRequest_NoTime()
        {
            var service = new ServiceTypeDto();

            var dto = new BookingRequestDto(service, Guid.NewGuid(), new PersonDto(), DateOnly.MinValue);

            var result = await BookingService.SendBookingRequest(dto, Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ClientError, result.ResultType);
            Assert.Contains(result.Errors, x => x.Contains("time"));
        }

        [Fact]
        public async void SendBookingRequest_ServiceUnavailable()
        {
            var date = new DateOnly(2024, 08, 30);

            BookingContext.Setup(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), date, date)).Returns(Task.FromResult<ICollection<Appointment>>([]));

            // Attempt to book 10:00 -> 11:30
            var service = new ServiceTypeDto()
            {
                DurationMins = 90,
                StartDate = DateTime.MinValue,
                BookingFrequencyMins = 30,
                EarliestTime = new TimeSpan(9, 0, 0),
                LatestTime = new TimeSpan(15, 0, 0),
                Repeats = [new ServiceRepeaterDto(1)],
                RepeatType = ServiceRepeaterTypeDto.MonthlyAbsolute
            };
            var dto = new BookingRequestDto(service, Guid.NewGuid(), new PersonDto(), date)
            {
                SelectedTime = new TimeOnly(10, 0),
            };

            var result = await BookingService.SendBookingRequest(dto, Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ClientError, result.ResultType);
            Assert.Contains(result.Errors, x => x.Contains("service"));

        }

        [Fact]
        public async void SendBookingRequest_Clashes()
        {
            var date = new DateOnly(2024, 08, 30);

            BookingContext.Setup(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), date, date)).Returns(Task.FromResult<ICollection<Appointment>>([
                new Appointment() { // No Clashes
                    StartTime = new DateTime(date, new TimeOnly(9, 0)),
                    EndTime = new DateTime(date, new TimeOnly(9, 30))
                },
                new Appointment() { // Clashes
                    StartTime = new DateTime(date, new TimeOnly(11, 0)),
                    EndTime = new DateTime(date, new TimeOnly(13, 0)),
                },
                new Appointment() { // Clashes
                    StartTime = new DateTime(date, new TimeOnly(15, 0)),
                    EndTime = new DateTime(date, new TimeOnly(15, 45)),
                }
                ]));


            // Attempt to book 10:00 -> 11:30
            var service = new ServiceTypeDto() { 
                DurationMins = 90, 
                StartDate = DateTime.MinValue, 
                BookingFrequencyMins = 30, 
                EarliestTime = new TimeSpan(9, 0, 0), 
                LatestTime = new TimeSpan(15, 0, 0),
                Repeats = [new ServiceRepeaterDto(30)],
                RepeatType = ServiceRepeaterTypeDto.MonthlyAbsolute
            };
            var dto = new BookingRequestDto(service, Guid.NewGuid(), new PersonDto(), date)
            {
                SelectedTime = new TimeOnly(10, 0),
            };

            var result = await BookingService.SendBookingRequest(dto, Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ClientError, result.ResultType);
            Assert.Contains(result.Errors, x => x.Contains("time"));
        }

        [Fact]
        public async void SendBookingRequest_Returns()
        {
            var date = new DateOnly(2024, 08, 30);

            BookingContext.Setup(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), date, date)).Returns(Task.FromResult<ICollection<Appointment>>([
                new Appointment() {
                    StartTime = new DateTime(date, new TimeOnly(9, 0)),
                    EndTime = new DateTime(date, new TimeOnly(10, 0))
                },
                new Appointment() {
                    StartTime = new DateTime(date, new TimeOnly(11, 30)),
                    EndTime = new DateTime(date, new TimeOnly(13, 0)),
                },
                new Appointment() {
                    StartTime = new DateTime(date, new TimeOnly(15, 0)),
                    EndTime = new DateTime(date, new TimeOnly(15, 45)),
                }
                ]));

            Appointment appointment;
            BookingContext.Setup(x => x.CreateBookingRequest(It.IsAny<Appointment>(), It.IsAny<Guid>())).Callback((Appointment app, Guid _) => appointment = app ).Returns(Task.FromResult(true));  

            // Attempt to book 10:00 -> 11:30
            var service = new ServiceTypeDto()
            {
                Guid = Guid.NewGuid(),
                Name = "Custom Service Name",
                DurationMins = 90,
                StartDate = DateTime.MinValue,
                BookingFrequencyMins = 30,
                EarliestTime = new TimeSpan(9, 0, 0),
                LatestTime = new TimeSpan(15, 0, 0),
                Repeats = [new ServiceRepeaterDto(30)],
                RepeatType = ServiceRepeaterTypeDto.MonthlyAbsolute
            };
            var person = new PersonDto() { FirstName = "Bob", LastName = "Smith", EmailAddress = "bob.smith@hotmail.com", PhoneNumber = "07123456789" };
            var dto = new BookingRequestDto(service, Guid.NewGuid(), person, date)
            {
                SelectedTime = new TimeOnly(10, 0),
            };

            var result = await BookingService.SendBookingRequest(dto, Guid.NewGuid());

            var expected = new AppointmentDto("Custom Service Name", person)
            {
                StartTime = new DateTime(date, new TimeOnly(10, 0)),
                EndTime = new DateTime(date, new TimeOnly(11, 30)),
                Service = service,
                BookingType = BookingTypeDto.ONLINE,
            };

            Assert.True(result.IsSuccess);
            Assert.Equal(ResultType.Success, result.ResultType);
            Assert.Equivalent(expected, result.Result);
        }

        [Fact]
        public async void SendBookingRequest_DatabaseFails()
        {
            var date = new DateOnly(2024, 08, 30);

            BookingContext.Setup(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), date, date)).Returns(Task.FromResult<ICollection<Appointment>>([
                new Appointment() {
                    StartTime = new DateTime(date, new TimeOnly(9, 0)),
                    EndTime = new DateTime(date, new TimeOnly(10, 0))
                },
                new Appointment() {
                    StartTime = new DateTime(date, new TimeOnly(11, 30)),
                    EndTime = new DateTime(date, new TimeOnly(13, 0)),
                },
                new Appointment() {
                    StartTime = new DateTime(date, new TimeOnly(15, 0)),
                    EndTime = new DateTime(date, new TimeOnly(15, 45)),
                }
                ]));

            BookingContext.Setup(x => x.CreateBookingRequest(It.IsAny<Appointment>(), It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Attempt to book 10:00 -> 11:30
            var service = new ServiceTypeDto()
            {
                Guid = Guid.NewGuid(),
                Name = "Custom Service Name",
                DurationMins = 90,
                StartDate = DateTime.MinValue,
                BookingFrequencyMins = 30,
                EarliestTime = new TimeSpan(9, 0, 0),
                LatestTime = new TimeSpan(15, 0, 0),
                Repeats = [new ServiceRepeaterDto(30)],
                RepeatType = ServiceRepeaterTypeDto.MonthlyAbsolute
            };
            var person = new PersonDto() { FirstName = "Bob", LastName = "Smith", EmailAddress = "bob.smith@hotmail.com", PhoneNumber = "07123456789" };
            var dto = new BookingRequestDto(service, Guid.NewGuid(), person, date)
            {
                SelectedTime = new TimeOnly(10, 0),
            };

            var result = await BookingService.SendBookingRequest(dto, Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ServerError, result.ResultType);
            Assert.Contains(result.Errors, x => x.Contains("server"));
        }

        [Fact]
        public async void SendBookingRequest_DatabaseThrows()
        {
            var date = new DateOnly(2024, 08, 30);

            BookingContext.Setup(x => x.GetBookingsBetweenDates(It.IsAny<Guid>(), date, date)).Returns(Task.FromResult<ICollection<Appointment>>([
                new Appointment() {
                    StartTime = new DateTime(date, new TimeOnly(9, 0)),
                    EndTime = new DateTime(date, new TimeOnly(10, 0))
                },
                new Appointment() {
                    StartTime = new DateTime(date, new TimeOnly(11, 30)),
                    EndTime = new DateTime(date, new TimeOnly(13, 0)),
                },
                new Appointment() {
                    StartTime = new DateTime(date, new TimeOnly(15, 0)),
                    EndTime = new DateTime(date, new TimeOnly(15, 45)),
                }
                ]));

            BookingContext.Setup(x => x.CreateBookingRequest(It.IsAny<Appointment>(), It.IsAny<Guid>())).Throws(new Exception());

            // Attempt to book 10:00 -> 11:30
            var service = new ServiceTypeDto()
            {
                Guid = Guid.NewGuid(),
                Name = "Custom Service Name",
                DurationMins = 90,
                StartDate = DateTime.MinValue,
                BookingFrequencyMins = 30,
                EarliestTime = new TimeSpan(9, 0, 0),
                LatestTime = new TimeSpan(15, 0, 0),
                Repeats = [new ServiceRepeaterDto(30)],
                RepeatType = ServiceRepeaterTypeDto.MonthlyAbsolute
            };
            var person = new PersonDto() { FirstName = "Bob", LastName = "Smith", EmailAddress = "bob.smith@hotmail.com", PhoneNumber = "07123456789" };
            var dto = new BookingRequestDto(service, Guid.NewGuid(), person, date)
            {
                SelectedTime = new TimeOnly(10, 0),
            };

            var result = await BookingService.SendBookingRequest(dto, Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ServerError, result.ResultType);
            Assert.Contains(result.Errors, x => x.Contains("server"));
        }
    }
}
