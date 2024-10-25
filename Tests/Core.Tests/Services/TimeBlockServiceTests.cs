using AutoMapper;
using Core.Dto;
using Core.Dto.Appointment;
using Core.Interfaces;
using Core.Mapping;
using Core.Responses;
using Core.Services;
using Data.Entity.Appointments;
using Data.Interfaces;
using Moq;

namespace Core.Tests.Services
{
    public class TimeBlockServiceTests
    {

        private readonly Mock<IAppointmentRepo> _appointmentRepo = new();

        private readonly IMapper _mapper;

        private readonly ITimeBlockService _timeBlockService;

        public TimeBlockServiceTests()
        {
            var mappingConfig = new MapperConfiguration(x => x.AddProfile(new AutoMapperConfig()));
            _mapper = mappingConfig.CreateMapper();

            _appointmentRepo.Setup(a => a.Create(It.IsAny<TimeBlock>())).ReturnsAsync(true);

            _timeBlockService = new TimeBlockService(_appointmentRepo.Object, _mapper);
        }

        [Theory]
        [InlineData(RepeaterTypeDto.Weekly)]
        [InlineData(RepeaterTypeDto.MonthlyAbsolute)]
        [InlineData(RepeaterTypeDto.MonthlyRelative)]
        public async void Create_RepeatTypeNoRepeats(RepeaterTypeDto repeaterType)
        {
            var tb = new TimeBlockDto("Example Time Block")
            {
                RepeatType = repeaterType
            };

            var result = await _timeBlockService.CreateOrUpdateTimeBlock(tb);

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ClientError, result.ResultType);
            Assert.Contains(result.Errors, x => x.Contains("repeats"));

            _appointmentRepo.Verify(a => a.Create(It.IsAny<TimeBlock>()), Times.Never());
            _appointmentRepo.Verify(a => a.Update(It.IsAny<TimeBlock>()), Times.Never());
        }

        [Fact]
        public async void Create_MapsCorrectly()
        {
            TimeBlock entity = new();
            _appointmentRepo.Setup(a => a.Create(It.IsAny<TimeBlock>())).Callback<TimeBlock>((timeBlock) => entity = timeBlock).ReturnsAsync(true);

            var tb = new TimeBlockDto("New Time Block")
            {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1),
                RepeatType = RepeaterTypeDto.Weekly,
                Repeats = [
                    new RepeaterDto(DayOfWeek.Monday),
                    new RepeaterDto(DayOfWeek.Wednesday),
                    new RepeaterDto(DayOfWeek.Sunday),
                ]
            };

            var expectedEntity = _mapper.Map<TimeBlock>(tb);
            var result = await _timeBlockService.CreateOrUpdateTimeBlock(tb);

            Assert.True(result.IsSuccess);
            Assert.Equivalent(expectedEntity, entity);
            _appointmentRepo.Verify(a => a.Create(It.IsAny<TimeBlock>()), Times.Once);
        }

        [Fact]
        public async void Create_DatabaseFails()
        {
            _appointmentRepo.Setup(a => a.Create(It.IsAny<TimeBlock>())).ReturnsAsync(false);

            var tb = new TimeBlockDto("New Time Block");
            var result = await _timeBlockService.CreateOrUpdateTimeBlock(tb);

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ServerError, result.ResultType);
            Assert.Null(result.Result);
            _appointmentRepo.Verify(a => a.Create(It.IsAny<TimeBlock>()), Times.Once);
        }

        [Fact]
        public async void Update_MapsCorrectly()
        {
            var guid = Guid.NewGuid();
            TimeBlock entity = new();
            _appointmentRepo.Setup(a => a.Update(It.IsAny<TimeBlock>())).Callback<TimeBlock>((timeBlock) => entity = timeBlock).ReturnsAsync(true);

            var tb = new TimeBlockDto("New Time Block")
            {
                Guid = guid,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1),
                RepeatType = RepeaterTypeDto.Weekly,
                Repeats = [
                    new RepeaterDto(DayOfWeek.Monday),
                    new RepeaterDto(DayOfWeek.Wednesday),
                    new RepeaterDto(DayOfWeek.Sunday),
                ]
            };

            var expectedEntity = _mapper.Map<TimeBlock>(tb);
            var result = await _timeBlockService.CreateOrUpdateTimeBlock(tb);

            Assert.True(result.IsSuccess);
            Assert.Equivalent(expectedEntity, entity);
            _appointmentRepo.Verify(a => a.Update(It.Is<TimeBlock>(tb => tb.Guid == guid)), Times.Once);
        }

        [Fact]
        public async void Update_DatabaseFails()
        {
            _appointmentRepo.Setup(a => a.Update(It.IsAny<TimeBlock>())).ReturnsAsync(false);

            var guid = Guid.NewGuid();
            var tb = new TimeBlockDto("New Time Block")
            {
                Guid = guid
            };
            var result = await _timeBlockService.CreateOrUpdateTimeBlock(tb);

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ServerError, result.ResultType);
            Assert.Null(result.Result);
            _appointmentRepo.Verify(a => a.Update(It.Is<TimeBlock>(tb => tb.Guid == guid)), Times.Once);
        }

        [Fact]
        public async void GetTimeBlock_NotFound_ClientError()
        {
            _appointmentRepo.Setup(a => a.GetTimeBlock(It.IsAny<Guid>())).ReturnsAsync((TimeBlock?)null);

            var guid = Guid.NewGuid();

            var result = await _timeBlockService.GetTimeBlock(guid);

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ClientError, result.ResultType);

            _appointmentRepo.Verify(a => a.GetTimeBlock(guid), Times.Once);
        }

        [Fact]
        public async void GetTimeBlock_MapsCorrectly()
        {
            var guid = Guid.NewGuid();

            var tb = new TimeBlock()
            {
                Id = 94,
                BusinessId = 17,
                Guid = guid,
                StartTime = new DateTime(2024, 10, 1, 10, 0, 0), // Starts Tuesday 10:00
                EndTime = new DateTime(2024, 10, 1, 12, 30, 0), // Ends Tuesday 12:30
                Name = "Test Time Block with exepctions",
                Exceptions = [
                    new(){
                        Name = "Updated Expection Name",
                        StartTime = new DateTime(2024, 10, 15, 10, 30, 0), // Moved a day forward and start at 10:30,
                        EndTime = new DateTime(2024, 10, 15, 12, 30, 0),
                        DateToReplace = new DateOnly(2024, 10, 15), // Change the 15th
                        TimeBlockId = 94
                    },
                    new(){
                        Name = "Updated Expection Name 2",
                        StartTime = new DateTime(2024, 10, 29, 10, 30, 0), // Moved a day forward and start at 10:30,
                        EndTime = new DateTime(2024, 10, 29, 12, 30, 0),
                        DateToReplace = new DateOnly(2024, 10, 29), // Change the 29th
                        TimeBlockId = 94
                    }
                    ],
                RepeatType = Data.Entity.RepeatType.Weekly,
                Repeats = [new() {
                    DayIdentifier = (int) DayOfWeek.Tuesday,
                    TimeBlockId = 94,
                }]
            };

            _appointmentRepo.Setup(a => a.GetTimeBlock(guid)).ReturnsAsync(tb);

            var result = await _timeBlockService.GetTimeBlock(guid);

            Assert.True(result.IsSuccess);
            Assert.Equivalent(_mapper.Map<TimeBlockDto>(tb), result.Result);

            _appointmentRepo.Verify(a => a.GetTimeBlock(guid), Times.Once);

        }

        [Fact]
        public async void GetTimeBlocksBetweenDates_NoneReturnsEmpty()
        {
            var timeBlocks = new List<TimeBlock>
            {
                new()
                {
                    Name = "TB1",
                    StartTime = new DateTime(2024, 11, 1, 11, 0, 0),
                    EndTime = new DateTime(2024, 11, 1, 12, 30, 0),
                    RepeatType = null  // Starts in the future
                },

                new()
                {
                    Name = "TB2",
                    StartTime = new DateTime(2024, 10, 1, 11, 0, 0),
                    EndTime = new DateTime(2024, 10, 1, 12, 30, 0),
                    RepeatType = null  // Does NOT repeat
                },

                new()
                {
                    Name = "TB3",
                    StartTime = new DateTime(2024, 10, 2, 10, 0, 0),
                    EndTime = new DateTime(2024, 10, 2, 15, 0, 0),
                    RepeatType = Data.Entity.RepeatType.Weekly,
                    Repeats = [
                        new() {
                            DayIdentifier = (int) DayOfWeek.Tuesday,
                        },
                        new () {
                            DayIdentifier = (int) DayOfWeek.Wednesday
                        }
                        ]
                },

                new()
                {
                    Name = "TB4",
                    StartTime = new DateTime(2024, 10, 2, 10, 0, 0),
                    EndTime = new DateTime(2024, 10, 2, 15, 0, 0),
                    RepeatType = Data.Entity.RepeatType.MonthlyAbsolute,
                    Repeats = [
                        new() {
                            DayIdentifier = 15,
                        },
                        new () {
                            DayIdentifier = 16
                        }
                        ]
                },

                new()
                {
                    Name = "TB5",
                    StartTime = new DateTime(2024, 10, 2, 10, 0, 0),
                    EndTime = new DateTime(2024, 10, 2, 15, 0, 0),
                    RepeatType = Data.Entity.RepeatType.MonthlyRelative,
                    Repeats = [
                        new() {
                            DayIdentifier = (int) DayOfWeek.Wednesday,
                            Index = 1,
                        },
                        new () {
                            DayIdentifier = (int) DayOfWeek.Sunday,
                            Index = -1,
                        }
                        ],
                    Exceptions = [
                        new() {
                            DateToReplace = new DateOnly(2024, 10, 3), // Replacing the first Wednesday of the sequence
                            StartTime = new DateTime(2024, 10, 3, 9, 0, 0), // Shifted hour earlier
                            EndTime = new DateTime(2024, 10, 3, 14, 0, 0) // Shfited hour earlier
                        }
                        ]
                }
            };

            _appointmentRepo.Setup(a => a.GetTimeBlocks()).ReturnsAsync(timeBlocks);


            var result = await _timeBlockService.GetTimeBlocksBetweenDates(new DateOnly(2024, 10, 4), new DateOnly(2024, 10, 7));

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Result!);
        }

        [Fact]
        public async void GetTimeBlocksBetweenDates()
        {
            Guid[] guids = new Guid[7];
            for (int i = 0; i < guids.Length; i++)
            {
                guids[i] = Guid.NewGuid();
            }

            var timeBlocks = new List<TimeBlock>()
            {
                // One-off, outside range
                new()
                {
                    Guid = guids[0],
                    Name = "TB0",
                    StartTime = new DateTime(2024, 10, 1, 10, 0, 0),
                    EndTime = new DateTime(2024, 10, 1, 10, 30, 0),
                    RepeatType = null

                },

                // One-off, inside
                new()
                {
                    Guid = guids[1],
                    Name = "TB1",
                    StartTime = new DateTime(2024, 10, 15, 10, 0, 0),
                    EndTime = new DateTime(2024, 10, 15, 10, 30, 0),
                    RepeatType = null
                },

                // One-off, straddle range start
                new()
                {
                    Guid = guids[2],
                    Name = "TB2",
                    StartTime = new DateTime(2024, 10, 9, 10, 0, 0),
                    EndTime = new DateTime(2024, 10, 10, 10, 30, 0),
                    RepeatType = null
                },

                // One-off, straddle range end
                new()
                {
                    Guid = guids[3],
                    Name = "TB3",
                    StartTime = new DateTime(2024, 10, 20, 10, 0, 0),
                    EndTime = new DateTime(2024, 10, 21, 10, 30, 0),
                    RepeatType = null
                },

                // Weekly, with exception
                new()
                {
                    Guid = guids[4],
                    Name = "TB4",
                    StartTime = new DateTime(2024, 10, 1, 15, 0, 0),
                    EndTime = new DateTime(2024, 10, 1, 17, 30, 0),
                    RepeatType = Data.Entity.RepeatType.Weekly,
                    Repeats = [
                        new() {
                            DayIdentifier = (int) DayOfWeek.Thursday
                        },
                        new() {
                            DayIdentifier = (int) DayOfWeek.Friday
                        }
                        ],
                    Exceptions = [
                        new() {
                            Name = "SHOULD NOT BE RETURNED",
                            DateToReplace = new DateOnly(2024, 10, 17),
                            StartTime = new DateTime(2024, 10, 17, 0, 0, 0),
                            EndTime = new DateTime(2024, 10, 17, 0, 0, 0) // 0 duration => deleted instance
                        },
                        new () {
                            Name = "TB4 - Moved",
                            DateToReplace = new DateOnly(2024, 10, 18),
                            StartTime = new DateTime(2024, 10, 18, 16, 0, 0),
                            EndTime = new DateTime(2024, 10, 18, 18, 0, 0)
                        }
                        ]
                },

                // Monthly absolute, with exception
                new()
                {
                    Guid = guids[5],
                    Name = "TB5",
                    StartTime = new DateTime(2024, 10, 1, 15, 0, 0),
                    EndTime = new DateTime(2024, 10, 1, 17, 30, 0),
                    RepeatType = Data.Entity.RepeatType.MonthlyAbsolute,
                    Repeats = [
                        new() { DayIdentifier = 1 },
                        new() { DayIdentifier = 9 },
                        new() { DayIdentifier = 15 }
                        ],
                    Exceptions = [
                        new () {
                            Name = "TB5 - MOVED INSIDE RANGE",
                            DateToReplace = new DateOnly(2024, 10, 1),
                            StartTime = new DateTime(2024, 10, 18, 16, 0, 0), // Moved inside range
                            EndTime = new DateTime(2024, 10, 18, 18, 0, 0)
                        }
                        ]
                },

                // Monthly Relative, with exception
                new()
                {
                    Guid = guids[6],
                    Name = "TB6",
                    StartTime = new DateTime(2024, 10, 1, 15, 0, 0),
                    EndTime = new DateTime(2024, 10, 1, 17, 30, 0),
                    RepeatType = Data.Entity.RepeatType.MonthlyRelative,
                    Repeats = [
                        new() {
                            DayIdentifier = (int) DayOfWeek.Friday,
                            Index = 2,
                        },
                        new() {
                            DayIdentifier = (int) DayOfWeek.Friday,
                            Index = 3,
                        },
                        new() {
                            DayIdentifier = (int) DayOfWeek.Friday,
                            Index = -1,
                        }
                        ],
                    Exceptions = [
                        new() {
                            Name = "Moved within",
                            DateToReplace = new DateOnly(2024, 10, 11),
                            StartTime = new DateTime(2024, 10, 11, 18, 0, 0),
                            EndTime = new DateTime(2024, 10, 11, 22, 0, 0)
                        },
                        new () {
                            Name = "MOVED OUTSIDE RANGE",
                            DateToReplace = new DateOnly(2024, 10, 18),
                            StartTime = new DateTime(2024, 10, 25, 16, 0, 0), // Moved outside range
                            EndTime = new DateTime(2024, 10, 25, 18, 0, 0)
                        }
                        ]
                },
            };

            var start = new DateOnly(2024, 10, 9);
            var end = new DateOnly(2024, 10, 20);


            _appointmentRepo.Setup(a => a.GetTimeBlocks()).ReturnsAsync(timeBlocks);

            /*
                EXPECTED:
            TB0 => 0
            TB1 => 1
            TB2 => 1
            TB3 => 1
            TB4 => 3 (inc 1 remove, 1 changed)
            TB5 => 3 (inc 1 moved inside)
            TB6 => 1 (inc 1 moved outside, 1 moved within)
            TOTAL 10
             */

            var expectedInstances = new List<TimeBlockInstanceDto>()
            {
                // TB1
                new(guids[1], "TB1", DateOnly.FromDateTime(timeBlocks[1].StartTime))
                {
                   StartTime = timeBlocks[1].StartTime,
                   EndTime = timeBlocks[1].EndTime,
                },
                // TB2
                new(guids[2], "TB2", DateOnly.FromDateTime(timeBlocks[2].StartTime))
                {
                   StartTime = timeBlocks[2].StartTime,
                   EndTime = timeBlocks[2].EndTime,
                },
                // TB3
                new(guids[3], "TB3", DateOnly.FromDateTime(timeBlocks[3].StartTime))
                {
                   StartTime = timeBlocks[3].StartTime,
                   EndTime = timeBlocks[3].EndTime,
                },
                // TB4
                new(guids[4], "TB4", new DateOnly(2024, 10, 10))
                {
                    StartTime = new DateTime(2024, 10, 10, 15, 0, 0),
                    EndTime = new DateTime(2024, 10, 10, 17, 30, 0),
                },
                new(guids[4], "TB4", new DateOnly(2024, 10, 11))
                {
                    StartTime = new DateTime(2024, 10, 11, 15, 0, 0),
                    EndTime = new DateTime(2024, 10, 11, 17, 30, 0),
                },
                new(guids[4], "TB4 - Moved", new DateOnly(2024, 10, 18), IsException: true) // Moved same day
                {
                    StartTime = new DateTime(2024, 10, 18, 16, 0, 0), // Moved forward vs timeBlock
                    EndTime = new DateTime(2024, 10, 18, 18, 00, 0), // Moved forward vs timeBlock
                },
                //TB5
                new(guids[5], "TB5 - MOVED INSIDE RANGE", new DateOnly(2024, 10, 1), IsException: true) // Moved from 1st to 18th
                {
                    StartTime = new DateTime(2024, 10, 18, 16, 0, 0),
                    EndTime = new DateTime(2024, 10, 18, 18, 0, 0),
                },
                new(guids[5], "TB5", new DateOnly(2024, 10, 9))
                {
                    StartTime = new DateTime(2024, 10, 9, 15, 0, 0),
                    EndTime = new DateTime(2024, 10, 9, 17, 30, 0),
                },
                new(guids[5], "TB5", new DateOnly(2024, 10, 15))
                {
                    StartTime = new DateTime(2024, 10, 15, 15, 0, 0),
                    EndTime = new DateTime(2024, 10, 15, 17, 30, 0),
                },
                //TB6
                new(guids[6], "Moved within", new DateOnly(2024, 10, 11), IsException: true)
                {
                    StartTime = new DateTime(2024, 10, 11, 18, 0, 0),
                    EndTime = new DateTime(2024, 10, 11, 22, 0, 0)
                },
            };

            var result = await _timeBlockService.GetTimeBlocksBetweenDates(start, end);
            var instances = result.Result!.ToList();


            Assert.True(result.IsSuccess);
            Assert.Equal(expectedInstances.Count, instances.Count);

            Assert.DoesNotContain(instances, i => i.TimeBlockGuid == guids[0]);

            Assert.Equivalent(expectedInstances[0], instances[0]);

            foreach (var item in expectedInstances)
            {
                var matchedInstance = instances.Single(i => i.TimeBlockGuid == item.TimeBlockGuid && i.InstanceDate == item.InstanceDate);
                Assert.Equivalent(item, matchedInstance);
            }
        }
    }
}
