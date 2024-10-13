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
    }
}
