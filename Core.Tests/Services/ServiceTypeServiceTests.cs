using AutoMapper;
using Core.Dto;
using Core.Dto.Services;
using Core.Interfaces;
using Core.Mapping;
using Core.Responses;
using Core.Services;
using Data.Entity;
using Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Core.Tests.Services
{
    public class ServiceTypeServiceTests
    {
        private readonly Mock<IServiceRepo> ServiceContextMock = new();
        private readonly Mock<IBusinessRepo> BusinessContextMock = new();
        private readonly Mock<IUserServiceInternal> UserManager = new();

        private IServiceTypeService ServiceTypeService;


        public ServiceTypeServiceTests()
        {
            var mappingConfig = new MapperConfiguration(x => x.AddProfile(new AutoMapperConfig()));
            var mapper = mappingConfig.CreateMapper();

            ServiceTypeService = new ServiceTypeService(ServiceContextMock.Object, BusinessContextMock.Object, UserManager.Object, mapper);

        }

        [Fact]
        public async void CreateService_NullUser_Fails()
        {
            UserManager.Setup(x => x.GetUserIdAsync()).Returns(Task.FromResult(""));

            var result = await ServiceTypeService.CreateOrUpdateServiceType(new ServiceTypeDto());

            Assert.False(result.IsSuccess);
            Assert.Equal(Responses.ResultType.ClientError, result.ResultType);

            // Check that no database call was made
            BusinessContextMock.Verify(x => x.GetBusinessForUser(It.IsAny<string>()), Times.Never);
            ServiceContextMock.Verify(x => x.Create(It.IsAny<string>(), It.IsAny<Service>()), Times.Never);
            ServiceContextMock.Verify(x => x.Update(It.IsAny<string>(), It.IsAny<Service>()), Times.Never);
        }

        [Fact]
        public async void CreateService_UserNoBusiness_Fails()
        {
            UserManager.Setup(x => x.GetUserIdAsync()).Returns(Task.FromResult("123-example-id-123"));
            BusinessContextMock.Setup(x => x.GetBusinessForUser(It.IsAny<string>())).Returns(Task.FromResult<Business?>(null));
            ServiceContextMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<Service>())).Returns(Task.FromResult(false));
            ServiceContextMock.Setup(x => x.Update(It.IsAny<string>(), It.IsAny<Service>())).Returns(Task.FromResult(false));

            var result = await ServiceTypeService.CreateOrUpdateServiceType(new ServiceTypeDto());

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ClientError, result.ResultType);

            // Check that no database call was made
            BusinessContextMock.Verify(x => x.GetBusinessForUser("123-example-id-123"), Times.Once);
            ServiceContextMock.Verify(x => x.Create(It.IsAny<string>(), It.IsAny<Service>()), Times.Never);
            ServiceContextMock.Verify(x => x.Update(It.IsAny<string>(), It.IsAny<Service>()), Times.Never);
        }

        [Fact]
        public async void CreateService_NoGuid_CallsCreate()
        {
            Service? createdService = null;
            string? userIdParameter = null;

            var userId = "Some random user Id as string";

            UserManager.Setup(x => x.GetUserIdAsync()).Returns(Task.FromResult(userId));
            BusinessContextMock.Setup(x => x.GetBusinessForUser(It.IsAny<string>())).Returns(Task.FromResult<Business?>(new Business() { Id = 7 }));
            ServiceContextMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<Service>()))
                .Callback<string, Service>((id, obj) =>
                {
                    userIdParameter = id;
                    createdService = obj;

                })
                .Returns(Task.FromResult(true));


            var result = await ServiceTypeService.CreateOrUpdateServiceType(new ServiceTypeDto());

            Assert.True(result.IsSuccess);
            Assert.Equal(userIdParameter!, userId!);
            Assert.Equal(7, createdService!.BusinessId);

            // Check database calls
            BusinessContextMock.Verify(x => x.GetBusinessForUser(userId), Times.Once);
            ServiceContextMock.Verify(x => x.Create(userId, It.IsAny<Service>()), Times.Once);
            ServiceContextMock.Verify(x => x.Update(It.IsAny<string>(), It.IsAny<Service>()), Times.Never);
        }

        [Fact]
        public async void CreateService_Guid_CallsUpdate()
        {
            Service? createdService = null;
            string? userIdParameter = null;

            var userId = "Some random user Id as string";

            UserManager.Setup(x => x.GetUserIdAsync()).Returns(Task.FromResult(userId));
            BusinessContextMock.Setup(x => x.GetBusinessForUser(It.IsAny<string>())).Returns(Task.FromResult<Business?>(new Business() { Id = 7 }));
            ServiceContextMock.Setup(x => x.Update(It.IsAny<string>(), It.IsAny<Service>()))
                .Callback<string, Service>((id, obj) =>
                {
                    userIdParameter = id;
                    createdService = obj;

                })
                .Returns(Task.FromResult(true));

            var serviceGuid = Guid.NewGuid().ToString();
            var result = await ServiceTypeService.CreateOrUpdateServiceType(new ServiceTypeDto()
            {
                Guid = Guid.Parse(serviceGuid),
                BookingFrequencyMins = 12,
                Duration = TimeSpan.FromMinutes(17),
                EarliestTime = TimeSpan.FromMinutes(90),
                LatestTime = TimeSpan.FromMinutes(643),
                Name = "My Custom Name",
                PaddingStartMins = 17,
                PaddingEndMins = 27,
                Price = 23.29m,
                RepeatType = ServiceRepeaterTypeDto.MonthlyAbsolute,
                Repeats = [
                    new ServiceRepeaterDto(3),
                    new ServiceRepeaterDto(17),
                    new ServiceRepeaterDto(29),
                    ]
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(userIdParameter!, userId!);
            Assert.Equal(7, createdService!.BusinessId);

            // Check database calls
            BusinessContextMock.Verify(x => x.GetBusinessForUser(userId), Times.Once);
            ServiceContextMock.Verify(x => x.Create(It.IsAny<string>(), It.IsAny<Service>()), Times.Never);
            ServiceContextMock.Verify(x => x.Update(userId, It.IsAny<Service>()), Times.Once);

            // Check the object used to call the service is mapped correctly
            Assert.NotNull(createdService);
            Assert.Equal(serviceGuid, createdService.Guid.ToString());
            Assert.Equal(12, createdService.BookingFrequencyMins);
            Assert.Equal(17, createdService.DurationMins);
            Assert.Equal(new TimeOnly(1, 30), createdService.EarliestTime);
            Assert.Equal(new TimeOnly(10, 43), createdService.LatestTime);
            Assert.Equal("My Custom Name", createdService.Name);
            Assert.Equal(17, createdService.PaddingStartMins);
            Assert.Equal(27, createdService.PaddingEndMins);
            Assert.Equal(23.29m, createdService.Price);
            Assert.Equal(ServiceRepeatType.MonthlyAbsolute, createdService.RepeatType);
            Assert.Equal(3, createdService.Repeats.Count);
            Assert.Contains(createdService.Repeats, x => x.DayIdentifier == 3 && x.Index == null);
            Assert.Contains(createdService.Repeats, x => x.DayIdentifier == 17 && x.Index == null);
            Assert.Contains(createdService.Repeats, x => x.DayIdentifier == 29 && x.Index == null);
        }

        [Fact]
        public async void GetServices_NoUserFails()
        {
            UserManager.Setup(x => x.GetUserIdAsync()).Returns(Task.FromResult(""));

            var result = await ServiceTypeService.GetServiceTypes();

            Assert.Null(result.Result);
            Assert.Equal(ResultType.ClientError, result.ResultType);
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, x => x.Contains("Unable to find id for user"));
        }

        [Fact]
        public async void GetServices_UserNoBusinessFails()
        {
            var userId = "Some random user Id as string";

            UserManager.Setup(x => x.GetUserIdAsync()).Returns(Task.FromResult(userId));
            BusinessContextMock.Setup(x => x.GetBusinessForUser(It.IsAny<string>())).Returns(Task.FromResult<Business?>(null));

            var result = await ServiceTypeService.GetServiceTypes();

            Assert.Null(result.Result);
            Assert.Equal(ResultType.ClientError, result.ResultType);
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, x => x.Contains("Unable to find business"));
        }

        [Fact]
        public async void GetServices_Returns()
        {
            var userId = "UserId";

            UserManager.Setup(x => x.GetUserIdAsync()).Returns(Task.FromResult(userId));
            BusinessContextMock.Setup(x => x.GetBusinessForUser("UserId")).Returns(Task.FromResult<Business?>(new Business()
            {
                Guid = Guid.NewGuid(),
                Id = 2
            }));
            ServiceContextMock.Setup(x => x.GetAllServicesForUser()).Returns(Task.FromResult(new List<Service>() {
                new Service()
                {
                    Id = 3,
                    Name = "Service 1",
                    DurationMins = 17,
                    LatestTime = new TimeOnly(17, 0)
                },

                new Service()
                {
                    Id=5,
                    BookingFrequencyMins = 25,
                    Name = "Second Service"
                }
           }.AsEnumerable()));


            var result = await ServiceTypeService.GetServiceTypes();

            Assert.Equal(ResultType.Success, result.ResultType);
            Assert.True(result.IsSuccess);

            Assert.Equal(2, result.Result!.Count);
            Assert.Contains(result.Result, x => x.Name == "Service 1");
            Assert.Contains(result.Result, x => x.Name == "Second Service");
        }

        [Fact]
        public async void GetServices_DatabaseError_Returns()
        {
            UserManager.Setup(x => x.GetUserIdAsync()).Returns(Task.FromResult("UserId"));
            BusinessContextMock.Setup(x => x.GetBusinessForUser("UserId")).Returns(Task.FromResult<Business?>(new Business()
            {
                Guid = Guid.NewGuid(),
                Id = 2
            }));

            ServiceContextMock.Setup(x => x.GetAllServicesForUser()).Throws(new InvalidOperationException());

            var result = await ServiceTypeService.GetServiceTypes();

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.Equal(ResultType.ServerError, result.ResultType);
            Assert.Contains(result.Errors, x => x.Contains("server error", StringComparison.CurrentCultureIgnoreCase));
        }

        [Fact]
        public async void DeleteService_Returns()
        {
            ServiceContextMock.Setup(x => x.Delete(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            var result = await ServiceTypeService.DeleteById(Guid.NewGuid());

            ServiceContextMock.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Once);

            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
        }

        [Fact]
        public async void CreateService_DatabaseError_Returns()
        {
            UserManager.Setup(x => x.GetUserIdAsync()).Returns(Task.FromResult("UserId"));
            BusinessContextMock.Setup(x => x.GetBusinessForUser("UserId")).Returns(Task.FromResult<Business?>(new Business()
            {
                Guid = Guid.NewGuid(),
                Id = 2
            }));

            ServiceContextMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<Service>())).Throws(new InvalidOperationException());

            var result = await ServiceTypeService.CreateOrUpdateServiceType(new ServiceTypeDto());

            ServiceContextMock.Verify(x => x.Create(It.IsAny<string>(), It.IsAny<Service>()), Times.Once);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.Equal(ResultType.ServerError, result.ResultType);
            Assert.Contains(result.Errors, x => x.Contains("server error", StringComparison.CurrentCultureIgnoreCase));
        }

        [Fact]
        public async void UpdateService_DatabaseError_Returns()
        {
            UserManager.Setup(x => x.GetUserIdAsync()).Returns(Task.FromResult("UserId"));
            BusinessContextMock.Setup(x => x.GetBusinessForUser("UserId")).Returns(Task.FromResult<Business?>(new Business()
            {
                Guid = Guid.NewGuid(),
                Id = 2
            }));

            ServiceContextMock.Setup(x => x.Update(It.IsAny<string>(), It.IsAny<Service>())).Throws(new InvalidOperationException());

            var result = await ServiceTypeService.CreateOrUpdateServiceType(new ServiceTypeDto() { Guid = Guid.NewGuid() });

            ServiceContextMock.Verify(x => x.Update(It.IsAny<string>(), It.IsAny<Service>()), Times.Once);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.Equal(ResultType.ServerError, result.ResultType);
            Assert.Contains(result.Errors, x => x.Contains("server error", StringComparison.CurrentCultureIgnoreCase));
        }

        [Fact]
        public async void DeleteService_DatabaseError_Returns()
        {
            UserManager.Setup(x => x.GetUserIdAsync()).Returns(Task.FromResult("UserId"));
            BusinessContextMock.Setup(x => x.GetBusinessForUser("UserId")).Returns(Task.FromResult<Business?>(new Business()
            {
                Guid = Guid.NewGuid(),
                Id = 2
            }));

            ServiceContextMock.Setup(x => x.Delete(It.IsAny<Guid>())).Throws(new Exception());

            var result = await ServiceTypeService.DeleteById(Guid.NewGuid());

            ServiceContextMock.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Once);

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultType.ServerError, result.ResultType);
            Assert.Contains(result.Errors, x => x.Contains("server error", StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
