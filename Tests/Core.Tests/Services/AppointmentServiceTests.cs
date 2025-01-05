using AutoMapper;
using Core.Interfaces;
using Core.Mapping;
using Core.Responses;
using Core.Services;
using Data.Interfaces;
using Moq;

namespace Core.Tests.Services
{
    public class AppointmentServiceTests
    {

        private readonly Mock<IAppointmentRepo> _appointmentRepo = new();

        private readonly IMapper _mapper;

        private readonly ITimeBlockService _timeBlockService;

        private readonly IAppointmentService _appointmentService;

        public AppointmentServiceTests()
        {
            var mappingConfig = new MapperConfiguration(x => x.AddProfile(new AutoMapperConfig()));
            _mapper = mappingConfig.CreateMapper();

            _timeBlockService = new TimeBlockService(_appointmentRepo.Object, _mapper);

            _appointmentService = new AppointmentService(_timeBlockService, _appointmentRepo.Object, _mapper);
        }

        [Fact]
        public async Task GetPendingAppointments()
        {
            await _appointmentService.GetPendingAppointments();

            _appointmentRepo.Verify(a => a.GetPendingAppointments(), Times.Once);
        }

        [Fact]
        public async Task GetPendingAppointments_DatabaseError()
        {
            _appointmentRepo.Setup(a => a.GetPendingAppointments()).ThrowsAsync(new Exception());

            var result = await _appointmentService.GetPendingAppointments();

            _appointmentRepo.Verify(a => a.GetPendingAppointments(), Times.Once);
            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ServerError, result.ResultType);
        }
    }
}
