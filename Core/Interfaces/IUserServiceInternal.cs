namespace Core.Interfaces
{
    internal interface IUserServiceInternal : IUserService
    {
        public Task<string> GetUserIdAsync();
    }
}
