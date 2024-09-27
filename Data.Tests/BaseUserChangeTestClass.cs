using Auth.Interfaces;
using Data.Context;
using Data.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Data.Tests
{
    public class BaseUserChangeTestClass : IClassFixture<DockerSqlFixture>, IDbContextFactory<ApplicationDbContext>
    {
        private readonly DockerSqlFixture _fixture;
        private readonly Mock<IUserService> _userServiceMock = new();

        public string MockUserId = "";

        public BaseUserChangeTestClass(DockerSqlFixture fixture)
        {
            _fixture = fixture;

            _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(() => MockUserId);
        }

        internal ApplicationDbContext GetContext() => _fixture.CreateContext();

        ApplicationDbContext IDbContextFactory<ApplicationDbContext>.CreateDbContext() => _fixture.CreateContext(_userServiceMock);
    }
}
