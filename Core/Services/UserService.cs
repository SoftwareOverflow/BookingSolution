using Auth.Interfaces;
using Core.Dto;
using Core.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Core.Services
{
    internal class UserService : IUserObserver, IDisposable, Interfaces.IUserService
    {
        private Auth.Interfaces.IUserService AuthUserService { get; set; }

        private IMessageService MessageService { get; set; }

        private UserStateManager UserStateManager { get; set; }

        /// <summary>
        /// Determines if the UserStateManager has up to date information.
        /// Call <seealso cref="UserService.Load"/> Load if this is false and up to date information is required
        /// </summary>
        public bool IsUpToDate { get; private set; } = false;

        /// <summary>
        /// Notifies with the current users first name whenever the current user changes, or null if the user is logged out.
        /// </summary>
        public event Action<string?>? OnUserChange;

        public UserService(Auth.Interfaces.IUserService userService, IMessageService messageService, UserStateManager userStateManager)
        {
            AuthUserService = userService;
            AuthUserService.AddUserListener(this);

            MessageService = messageService;
            UserStateManager = userStateManager;
        }

        public async void OnUserEvent(UserEvent userEvent, string userId)
        {
            // TODO handle business logic here
            // TODO send any required messages using the MessageService
            // TODO remove complex tasks to a separate async Task<T> method to allow for better error handling. Errors thrown in async void will NOT be caught at higher levels

            var name = string.Empty;

            if (userEvent == UserEvent.Created)
            {
                
                name = await AuthUserService.GetUserNameFromId(userId);

                // TODO create BusinessUser in db
                // TODO logging
                MessageService.AddMessage(new MessageBase("User Created Successfully", MessageBase.MessageType.Success));
            }
            else if (userEvent == UserEvent.SignIn)
            {
                name = await AuthUserService.GetUserNameFromId(userId);

                MessageService.AddMessage(new MessageBase("User Signed In", MessageBase.MessageType.Success));

                // TODO logging
            } else if(userEvent == UserEvent.SignOut)
            {
                userId = string.Empty;

                MessageService.AddMessage(new MessageBase("Signed Out Successfully", MessageBase.MessageType.Success));
                // TODO logging
            }

            Console.WriteLine($"\n\nUserService pushing updates to UserStateManager {UserStateManager.GetHashCode()}\n\n");
            UserStateManager.UpdateUser(userId, name);
            
            OnUserChange?.Invoke(UserStateManager.UserFirstName);

            IsUpToDate = true;
        }

        public string GetSignOutPage() => AuthUserService.GetSignOutPage();

        public async Task Load()
        {
            if (IsUpToDate)
            {
                return;
            }

            var id = await AuthUserService.GetCurrentUserId();
            var name = string.Empty;
            if(!id.IsNullOrEmpty())
            {
                name = await AuthUserService.GetUserNameFromId(id);
            }

            UserStateManager.UpdateUser(id, name);

            OnUserChange?.Invoke(UserStateManager.UserFirstName);

            IsUpToDate = true;
        }

        public void Dispose()
        {
            AuthUserService.RemoveUserListener(this);
        }
    }
}
