namespace Core.Interfaces
{
    public interface IUserStateManager
    {
        public event Action<string>? OnUserChange;

        public Task Load();
        public string GetSignOutPage();
    }
}
