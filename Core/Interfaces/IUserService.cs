namespace Core.Interfaces
{
    public interface IUserService
    {
        public event Action<string?>? OnUserChange;

        public Task Load();
        public string GetSignOutPage();
    }
}
