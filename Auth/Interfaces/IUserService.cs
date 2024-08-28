namespace Auth.Interfaces
{
    public interface IUserService
    {
        public void AddUserListener(IUserObserver observer);
        public void RemoveUserListener(IUserObserver observer);

        public  Task<string> GetUserNameFromId(string userId);

        public Task<string> GetCurrentUserIdAsync();

        public string GetCurrentUserId();

        public string GetSignOutPage();
    }

    public interface IUserObserver
    {
        public void OnUserEvent(UserEvent userEvent, string userId);
    }

    public enum UserEvent
    {
        Created, SignIn, SignOut
    }
}
