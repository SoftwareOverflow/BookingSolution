using AutoMapper;
using Core.Dto;
using Core.Interfaces;
using Core.Mapping;
using Core.Services;
using Data.Entity;
using Data.Interfaces;
using Moq;

namespace Core.Tests.Services
{
    public class ServiceTypeServiceTests
    {
        private static  readonly Mock<IServiceContext> ServiceContextMock = new();
        private static readonly Mock<IBusinessContext> BusinessContextMock = new();
        private static readonly UserStateManager UserStateManager = new();

        private static IServiceTypeService ServiceTypeService;


        static ServiceTypeServiceTests()
        {
            var mappingConfig = new MapperConfiguration(x => x.AddProfile(new AutoMapperConfig()));
            var mapper = mappingConfig.CreateMapper();

            ServiceTypeService = new ServiceTypeService(ServiceContextMock.Object, BusinessContextMock.Object, UserStateManager, mapper);

        }

        [Fact]
        public async void CreateService_NullUser_Fails()
        {
            UserStateManager.UpdateUser("", "");

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
            BusinessContextMock.Setup(x => x.GetBusinessForUser(It.IsAny<string>())).Returns(Task.FromResult<Business?>(null));

            var userId = Guid.NewGuid().ToString();
            UserStateManager.UpdateUser(userId, "Bob Smith");

            var result = await ServiceTypeService.CreateOrUpdateServiceType(new ServiceTypeDto());

            Assert.False(result.IsSuccess);
            Assert.Equal(Responses.ResultType.ClientError, result.ResultType);

            // Check that no database call was made
            BusinessContextMock.Verify(x => x.GetBusinessForUser(userId), Times.Once);
            ServiceContextMock.Verify(x => x.Create(It.IsAny<string>(), It.IsAny<Service>()), Times.Never);
            ServiceContextMock.Verify(x => x.Update(It.IsAny<string>(), It.IsAny<Service>()), Times.Never);
        }

        [Fact]
        public async void CreateService_NoGuid_CallsCreate()
        {
            Service? createdService = null;
            string? userIdParameter = null;
            
            BusinessContextMock.Setup(x => x.GetBusinessForUser(It.IsAny<string>())).Returns(Task.FromResult<Business?>(new Business() { Id = 7 }));
            ServiceContextMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<Service>()))
                .Callback<string, Service>((id, obj) =>
                {
                    userIdParameter = id;
                    createdService = obj;

                })
                .Returns(Task.FromResult(true));


            var userId = Guid.NewGuid().ToString();
            UserStateManager.UpdateUser(userId, "Bob Smith");

            var result = await ServiceTypeService.CreateOrUpdateServiceType(new ServiceTypeDto());

            Assert.True(result.IsSuccess);
            Assert.Equal(userIdParameter!, userId!);
            Assert.Equal(7, createdService!.BusinessId);

            // Check database calls
            BusinessContextMock.Verify(x => x.GetBusinessForUser(userId), Times.Once);
            ServiceContextMock.Verify(x => x.Create(It.IsAny<string>(), It.IsAny<Service>()), Times.Once);
            ServiceContextMock.Verify(x => x.Update(It.IsAny<string>(), It.IsAny<Service>()), Times.Never);
        }
    }
}
