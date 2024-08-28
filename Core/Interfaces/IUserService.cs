namespace Core.Interfaces
{
    public interface IUserService
    {
        public string GetSignOutPage();

        public Task<string?> GetUserNameAsync();
    }
}
