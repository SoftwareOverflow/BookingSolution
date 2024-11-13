using AutoMapper;
using Core.Dto;
using Core.Dto.Appointment;
using Core.Interfaces;
using Core.Mapping;
using Core.Responses;
using Core.Services;
using Data.Entity.Appointments;
using Data.Extensions;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;
using System;

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

            var result = await _timeBlockService.CreateOrUpdate(tb);

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
            var result = await _timeBlockService.CreateOrUpdate(tb);

            Assert.True(result.IsSuccess);
            Assert.Equivalent(expectedEntity, entity);
            _appointmentRepo.Verify(a => a.Create(It.IsAny<TimeBlock>()), Times.Once);
        }

        [Fact]
        public async void Create_DatabaseFails()
        {
            _appointmentRepo.Setup(a => a.Create(It.IsAny<TimeBlock>())).ReturnsAsync(false);

            var tb = new TimeBlockDto("New Time Block");
            var result = await _timeBlockService.CreateOrUpdate(tb);

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ServerError, result.ResultType);
            Assert.Null(result.Result);
            _appointmentRepo.Verify(a => a.Create(It.IsAny<TimeBlock>()), Times.Once);
        }

        [Fact]
        public async void Create_DatabaseException()
        {
            _appointmentRepo.Setup(a => a.Create(It.IsAny<TimeBlock>())).ThrowsAsync(new Exception());

            var tb = new TimeBlockDto("New Time Block");
            var result = await _timeBlockService.CreateOrUpdate(tb);

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
            var result = await _timeBlockService.CreateOrUpdate(tb);

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
            var result = await _timeBlockService.CreateOrUpdate(tb);

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ServerError, result.ResultType);
            Assert.Null(result.Result);
            _appointmentRepo.Verify(a => a.Update(It.Is<TimeBlock>(tb => tb.Guid == guid)), Times.Once);
        }

        [Fact]
        public async void Update_DatabaseException()
        {
            _appointmentRepo.Setup(a => a.Update(It.IsAny<TimeBlock>())).ThrowsAsync(new Exception());

            var guid = Guid.NewGuid();
            var tb = new TimeBlockDto("New Time Block")
            {
                Guid = guid
            };
            var result = await _timeBlockService.CreateOrUpdate(tb);

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
        public async void GetTimeBlock_DatabaseFails()
        {
            _appointmentRepo.Setup(a => a.GetTimeBlock(It.IsAny<Guid>())).ThrowsAsync(new Exception());

            var guid = Guid.NewGuid();

            var result = await _timeBlockService.GetTimeBlock(guid);

            Assert.False(result.IsSuccess);
            Assert.Equivalent(result, ServiceResult<TimeBlockDto>.DefaultServerFailure());

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
            _appointmentRepo.Setup(a => a.GetTimeBlockExceptionsBetweenDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).Returns([]);

            var result = await _timeBlockService.GetTimeBlocksBetweenDates(new DateOnly(2024, 10, 4), new DateOnly(2024, 10, 7));

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Result!);
        }

        [Fact]
        public async void GetTimeBlocksBetweenDates()
        {
            Guid[] guids = new Guid[8];
            for (int i = 0; i < guids.Length; i++)
            {
                guids[i] = Guid.NewGuid();
            }

            Guid[] exceptionGuids = new Guid[4];
            for (int i = 0; i < exceptionGuids.Length; i++)
            {
                exceptionGuids[i] = Guid.NewGuid();
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
                            EndTime = new DateTime(2024, 10, 18, 18, 0, 0),
                            Guid = exceptionGuids[0]
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
                            EndTime = new DateTime(2024, 10, 18, 18, 0, 0),
                            Guid = exceptionGuids[1],
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
                            EndTime = new DateTime(2024, 10, 11, 22, 0, 0),
                            Guid = exceptionGuids[2],
                        },
                        new () {
                            Name = "MOVED OUTSIDE RANGE",
                            DateToReplace = new DateOnly(2024, 10, 18),
                            StartTime = new DateTime(2024, 10, 25, 16, 0, 0), // Moved outside range
                            EndTime = new DateTime(2024, 10, 25, 18, 0, 0)
                        }
                        ]
                },

                new()
                {
                    Guid = guids[7],
                    Name = "TB7",
                    StartTime = new DateTime(2024, 11, 1, 12, 30, 0), // Starts outside range
                    EndTime = new DateTime(2024, 11, 1, 17, 30, 0),
                    RepeatType = Data.Entity.RepeatType.MonthlyAbsolute,
                    Repeats = [
                        new(){
                            DayIdentifier = 1,
                        }
                        ],
                    Exceptions = [
                        new() {
                            Name = "Sequence starts in future, but this one is moved inside the range",
                            DateToReplace = new DateOnly(2024, 12, 1),
                            StartTime = new DateTime(2024, 10, 13, 11, 30, 0),
                            EndTime = new DateTime(2024, 10, 13, 12, 30, 0),
                            Guid = exceptionGuids[3],
                        },
                        new() {
                            Name = "Sequence starts in future, but this one is moved BEFORE the range",
                            DateToReplace = new DateOnly(2025, 3, 1),
                            StartTime = new DateTime(2024, 1, 1, 11, 30, 0),
                            EndTime = new DateTime(2024, 1, 1, 12, 30, 0)
                        },
                        ]
                }
            };

            var start = new DateOnly(2024, 10, 9);
            var end = new DateOnly(2024, 10, 20);

            var exceptionDict = new Dictionary<Guid, ICollection<TimeBlockException>>();
            foreach (var tb in timeBlocks)
            {
                var exceptionsInRange = tb.Exceptions.AsQueryable().BetweenDates(start, end);

                if (exceptionsInRange.Any())
                {
                    exceptionDict.Add(tb.Guid, exceptionsInRange.ToList());
                }
            }

            _appointmentRepo.Setup(a => a.GetTimeBlocks()).ReturnsAsync(timeBlocks);
            _appointmentRepo.Setup(a => a.GetTimeBlockExceptionsBetweenDates(start, end)).Returns(exceptionDict);

            /*
                EXPECTED:
            TB0 => 0
            TB1 => 1
            TB2 => 1
            TB3 => 1
            TB4 => 3 (inc 1 remove, 1 changed)
            TB5 => 3 (inc 1 moved inside)
            TB6 => 1 (inc 1 moved outside, 1 moved within)
            TB7 => 1 (inc 1 moved insinde)
            TOTAL 11
             */

            var expectedInstances = new List<TimeBlockInstanceDto>()
            {
                // TB1
                new(guids[1], "TB1", DateOnly.FromDateTime(timeBlocks[1].StartTime), IsOneOff: true)
                {
                   StartTime = timeBlocks[1].StartTime,
                   EndTime = timeBlocks[1].EndTime,
                },
                // TB2
                new(guids[2], "TB2", DateOnly.FromDateTime(timeBlocks[2].StartTime), IsOneOff: true)
                {
                   StartTime = timeBlocks[2].StartTime,
                   EndTime = timeBlocks[2].EndTime,
                },
                // TB3
                new(guids[3], "TB3", DateOnly.FromDateTime(timeBlocks[3].StartTime), IsOneOff: true)
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
                    Guid = exceptionGuids[0],
                },
                //TB5
                new(guids[5], "TB5 - MOVED INSIDE RANGE", new DateOnly(2024, 10, 1), IsException: true) // Moved from 1st to 18th
                {
                    StartTime = new DateTime(2024, 10, 18, 16, 0, 0),
                    EndTime = new DateTime(2024, 10, 18, 18, 0, 0),
                    Guid = exceptionGuids[1],
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
                    EndTime = new DateTime(2024, 10, 11, 22, 0, 0),
                    Guid = exceptionGuids[2],
                },
                //TB7
                new(guids[7], "Sequence starts in future, but this one is moved inside the range", new DateOnly(2024, 12, 1), IsException: true) // Moved from 01/12 inside the range
                {
                    StartTime = new DateTime(2024, 10, 13, 11, 30, 0),
                    EndTime = new DateTime(2024, 10, 13, 12, 30, 0),
                    Guid = exceptionGuids[3],
                }
            };

            var result = await _timeBlockService.GetTimeBlocksBetweenDates(start, end);
            var instances = result.Result!.ToList();

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedInstances.Count, instances.Count);

            Assert.DoesNotContain(instances, i => i.TimeBlockGuid == guids[0]);
            Assert.DoesNotContain(instances, i => i.IsException && i.Guid == Guid.Empty); // All exceptions should have proper guids as they are their own entities

            Assert.Equivalent(expectedInstances[0], instances[0]);

            foreach (var item in expectedInstances)
            {
                var matchedInstance = instances.Single(i => i.TimeBlockGuid == item.TimeBlockGuid && i.InstanceDate == item.InstanceDate);
                Assert.Equivalent(item, matchedInstance);
            }
        }

        [Fact]
        public async void CreateTimeBlockException_ClientError()
        {
            _appointmentRepo.Setup(a => a.GetTimeBlock(It.IsAny<Guid>())).ReturnsAsync((TimeBlock?)null);
            _appointmentRepo.Setup(a => a.GetAppointmentsBetweenDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).Returns([]);

            var timeBlockEx = new TimeBlockExceptionDto("Example exception");

            var result = await _timeBlockService.CreateOrUpdate(timeBlockEx, Guid.NewGuid());

            Assert.Equal(ResultType.ClientError, result.ResultType);
        }

        [Fact]
        public async void CreateTimeBlockException_ServerError()
        {
            var tbGuid = Guid.NewGuid();
            _appointmentRepo.Setup(a => a.GetTimeBlock(It.IsAny<Guid>())).ReturnsAsync(
                new TimeBlock()
                {
                    Guid = tbGuid,
                });

            _appointmentRepo.Setup(a => a.GetTimeBlockExceptionsBetweenDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).Returns(new Dictionary<Guid, ICollection<TimeBlockException>>()
            {
                { tbGuid, [new TimeBlockException() {
                    Name = "Exception Name",
                    DateToReplace = new DateOnly(2024, 10, 3),
                    StartTime = new DateTime(2024, 10, 3, 12, 30, 0),
                    EndTime = new DateTime(2024, 10, 3, 13, 45, 0),
                }] }
            });

            _appointmentRepo.Setup(a => a.Create(It.IsAny<TimeBlockException>(), tbGuid)).ThrowsAsync(new Exception());

            var timeBlockEx = new TimeBlockExceptionDto("Example exception")
            {
                DateToReplace = new DateOnly(2024, 10, 3),
            };

            var result = await _timeBlockService.CreateOrUpdate(timeBlockEx, tbGuid);

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ServerError, result.ResultType);

            _appointmentRepo.Verify(a => a.Create(It.IsAny<TimeBlockException>(), tbGuid), Times.Once);
        }

        [Fact]
        public async void CreateTimeBlockException_InstancesFails()
        {
            _appointmentRepo.Setup(a => a.GetTimeBlock(It.IsAny<Guid>())).ReturnsAsync(
                new TimeBlock()
                {
                    Guid = Guid.NewGuid(),
                });
            _appointmentRepo.Setup(a => a.GetTimeBlockExceptionsBetweenDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).Throws(new Exception());

            var timeBlockEx = new TimeBlockExceptionDto("Example exception");

            var result = await _timeBlockService.CreateOrUpdate(timeBlockEx, Guid.NewGuid());

            Assert.Equal(ResultType.ServerError, result.ResultType);

            _appointmentRepo.Verify(a => a.Create(It.IsAny<TimeBlockException>(), It.IsAny<Guid>()), Times.Never);
            _appointmentRepo.Verify(a => a.Update(It.IsAny<TimeBlockException>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async void CreateTimeBlockException_NoInstanceMatch()
        {
            var tbGuid = Guid.NewGuid();

            _appointmentRepo.Setup(a => a.GetTimeBlock(It.IsAny<Guid>())).ReturnsAsync(
                new TimeBlock()
                {
                    Guid = tbGuid,
                });

            _appointmentRepo.Setup(a => a.GetTimeBlockExceptionsBetweenDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).Returns(new Dictionary<Guid, ICollection<TimeBlockException>>()
            {
                { tbGuid, [new TimeBlockException() {
                    Name = "Exception Name",
                    DateToReplace = new DateOnly(2024, 10, 3),
                    StartTime = new DateTime(2024, 10, 3, 12, 30, 0),
                    EndTime = new DateTime(2024, 10, 3, 13, 45, 0),
                }] }
            });

            _appointmentRepo.Setup(a => a.Create(It.IsAny<TimeBlockException>(), tbGuid)).ReturnsAsync(true);

            var newEx = new TimeBlockExceptionDto("New TBE")
            {
                DateToReplace = DateOnly.MinValue,
            };

            var result = await _timeBlockService.CreateOrUpdate(newEx, tbGuid);

            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Contains("date"));

            _appointmentRepo.Verify(a => a.Create(It.IsAny<TimeBlockException>(), tbGuid), Times.Never);
            _appointmentRepo.Verify(a => a.Update(It.IsAny<TimeBlockException>(), tbGuid), Times.Never);
        }

        [Fact]
        public async void CreateTimeBlockException_DatabaseFails()
        {
            var tbGuid = Guid.NewGuid();

            _appointmentRepo.Setup(a => a.GetTimeBlock(It.IsAny<Guid>())).ReturnsAsync(
                new TimeBlock()
                {
                    Guid = tbGuid,
                });

            _appointmentRepo.Setup(a => a.GetTimeBlockExceptionsBetweenDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).Returns(new Dictionary<Guid, ICollection<TimeBlockException>>()
            {
                { tbGuid, [new TimeBlockException() {
                    Name = "Exception Name",
                    DateToReplace = new DateOnly(2024, 10, 3),
                    StartTime = new DateTime(2024, 10, 3, 12, 30, 0),
                    EndTime = new DateTime(2024, 10, 3, 13, 45, 0),
                }] }
            });

            _appointmentRepo.Setup(a => a.Create(It.IsAny<TimeBlockException>(), tbGuid)).ReturnsAsync(false);

            var newEx = new TimeBlockExceptionDto("New TBE")
            {
                DateToReplace = new DateOnly(2024, 10, 3),
            };

            var result = await _timeBlockService.CreateOrUpdate(newEx, tbGuid);

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ServerError, result.ResultType);

            _appointmentRepo.Verify(a => a.Create(It.IsAny<TimeBlockException>(), tbGuid), Times.Once);
        }

        [Fact]
        public async void CreateTimeBlockException()
        {
            var tbGuid = Guid.NewGuid();

            _appointmentRepo.Setup(a => a.GetTimeBlock(It.IsAny<Guid>())).ReturnsAsync(
                new TimeBlock()
                {
                    Guid = tbGuid,
                });

            _appointmentRepo.Setup(a => a.GetTimeBlockExceptionsBetweenDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).Returns(new Dictionary<Guid, ICollection<TimeBlockException>>()
            {
                { tbGuid, [new TimeBlockException() {
                    Name = "Exception Name",
                    DateToReplace = new DateOnly(2024, 10, 3),
                    StartTime = new DateTime(2024, 10, 3, 12, 30, 0),
                    EndTime = new DateTime(2024, 10, 3, 13, 45, 0),
                }] }
            });

            _appointmentRepo.Setup(a => a.Create(It.IsAny<TimeBlockException>(), tbGuid)).ReturnsAsync(true);

            var newEx = new TimeBlockExceptionDto("New TBE")
            {
                DateToReplace = new DateOnly(2024, 10, 3),
            };

            var result = await _timeBlockService.CreateOrUpdate(newEx, tbGuid);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.NewGuid(), result.Result!.Guid);


            _appointmentRepo.Verify(a => a.Create(It.IsAny<TimeBlockException>(), tbGuid), Times.Once);
        }

        [Fact]
        public async void UpdateTimeBlockException()
        {
            var tbGuid = Guid.NewGuid();

            _appointmentRepo.Setup(a => a.GetTimeBlock(It.IsAny<Guid>())).ReturnsAsync(
                new TimeBlock()
                {
                    Guid = tbGuid,
                });

            _appointmentRepo.Setup(a => a.GetTimeBlockExceptionsBetweenDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).Returns(new Dictionary<Guid, ICollection<TimeBlockException>>()
            {
                { tbGuid, [new TimeBlockException() {
                    Name = "Exception Name",
                    DateToReplace = new DateOnly(2024, 10, 3),
                    StartTime = new DateTime(2024, 10, 3, 12, 30, 0),
                    EndTime = new DateTime(2024, 10, 3, 13, 45, 0),
                }] }
            });

            _appointmentRepo.Setup(a => a.Update(It.IsAny<TimeBlockException>(), tbGuid)).ReturnsAsync(true);

            var existingException = new TimeBlockExceptionDto("Existing TBE")
            {
                Guid = Guid.NewGuid(),
                DateToReplace = new DateOnly(2024, 10, 3),
            };

            var result = await _timeBlockService.CreateOrUpdate(existingException, tbGuid);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.NewGuid(), result.Result!.Guid);


            _appointmentRepo.Verify(a => a.Update(It.IsAny<TimeBlockException>(), tbGuid), Times.Once);
        }

        [Fact]
        public async void DeleteTimeBlockException()
        {
            var guid = Guid.NewGuid();
            var instanceSpecificGuid = Guid.NewGuid();
            var instance = new TimeBlockInstanceDto(guid, "Exception to delete", new DateOnly(2024, 10, 2), IsException: true)
            {
                Guid = instanceSpecificGuid
            };

            _appointmentRepo.Setup(a => a.DeleteException(It.IsAny<TimeBlockException>(), guid)).ReturnsAsync(true);

            var result = await _timeBlockService.Delete(instance, false);

            Assert.True(result.IsSuccess);
            _appointmentRepo.Verify(a => a.DeleteException(It.Is<TimeBlockException>(tbe => tbe.Guid == instanceSpecificGuid), guid), Times.Once);
        }

        [Fact]
        public async void DeleteTimeBlockException_DatabaseFails()
        {
            var guid = Guid.NewGuid();
            var instanceSpecificGuid = Guid.NewGuid();
            var instance = new TimeBlockInstanceDto(guid, "Exception to delete", new DateOnly(2024, 10, 2), IsException: true)
            {
                Guid = instanceSpecificGuid
            };

            _appointmentRepo.Setup(a => a.DeleteException(It.IsAny<TimeBlockException>(), guid)).ReturnsAsync(false);

            var result = await _timeBlockService.Delete(instance, true);

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ServerError, result.ResultType);

            _appointmentRepo.Verify(a => a.DeleteException(It.Is<TimeBlockException>(tbe => tbe.Guid == instanceSpecificGuid), guid), Times.Once);
        }

        [Fact]
        public async void DeleteTimeBlockException_Errors()
        {
            var guid = Guid.NewGuid();
            var instanceSpecificGuid = Guid.NewGuid();
            var instance = new TimeBlockInstanceDto(guid, "Exception to delete", new DateOnly(2024, 10, 2), IsException: true)
            {
                Guid = instanceSpecificGuid
            };

            _appointmentRepo.Setup(a => a.DeleteException(It.IsAny<TimeBlockException>(), guid)).ThrowsAsync(new Exception());

            var result = await _timeBlockService.Delete(instance, true);

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ServerError, result.ResultType);

            _appointmentRepo.Verify(a => a.DeleteException(It.Is<TimeBlockException>(tbe => tbe.Guid == instanceSpecificGuid), guid), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void DeleteTimeBlock(bool deleteExceptions)
        {
            var guid = Guid.NewGuid();
            var instance = new TimeBlockInstanceDto(guid, "Custom Time Block Instance", new DateOnly(2024, 10, 1), IsException: false);

            _appointmentRepo.Setup(a => a.DeleteTimeBlock(guid, deleteExceptions)).ReturnsAsync(true);

            var result = await _timeBlockService.Delete(instance, deleteExceptions);

            Assert.True(result.IsSuccess);
            _appointmentRepo.Verify(a => a.DeleteTimeBlock(guid, deleteExceptions), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void DeleteTimeBlock_DatabaseFails(bool deleteExceptions)
        {
            var guid = Guid.NewGuid();
            var instance = new TimeBlockInstanceDto(guid, "Custom Time Block Instance", new DateOnly(2024, 10, 1), IsException: false);

            _appointmentRepo.Setup(a => a.DeleteTimeBlock(guid, deleteExceptions)).ReturnsAsync(false);

            var result = await _timeBlockService.Delete(instance, deleteExceptions);

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ServerError, result.ResultType);

            _appointmentRepo.Verify(a => a.DeleteTimeBlock(guid, deleteExceptions), Times.Once);
        }
    }
}
