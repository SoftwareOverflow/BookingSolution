namespace Core.Interfaces
{
    public interface IUserService
    {
        public event Action<string?>? OnUserChange;

        public string GetSignOutPage();

        public Task<string?> GetUserName();
    }
}
