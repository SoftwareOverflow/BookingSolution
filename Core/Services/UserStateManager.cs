using Auth.Interfaces;
using Core.Dto;
using Core.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Core.Services
{
    internal class UserStateManager : IUserObserver, IDisposable, IUserStateManager
    {
        private IUserService UserService { get; set; }

        private IMessageService MessageService { get; set; }

        /// <summary>
        /// Determines if the UserStateManager has up to date information.
        /// Call <seealso cref="UserStateManager.Load"/> Load if this is false and up to date information is required
        /// </summary>
        public bool IsUpToDate { get; private set; } = false;

        /// <summary>
        /// Unique reference to the user
        /// </summary>
        internal string? UserId { get; private set; } = null;

        /// <summary>
        /// The first name of the user, usable for a personalized experience when greeting the user for example
        /// </summary>
        public string? UserFirstName { get; private set; } = null;

        public event Action<string?>? OnUserChange;

        public UserStateManager(IUserService userService, IMessageService messageService)
        {
            UserService = userService;
            UserService.AddUserListener(this);

            MessageService = messageService;
        }

        public async void OnUserEvent(UserEvent userEvent, string userId)
        {
            // TODO handle business logic here
            // TODO send any required messages using the MessageService
            // TODO remove complex tasks to a separate async Task<T> method to allow for better error handling. Errors thrown in async void will NOT be caught at higher levels

            if (userEvent == UserEvent.Created)
            {
                UserId = userId;
                UserFirstName = await UserService.GetUserNameFromId(userId);

                // TODO create BusinessUser in db
                // TODO logging
                MessageService.AddMessage(new MessageBase("User Created Successfully", MessageBase.MessageType.Success));
            }
            else if (userEvent == UserEvent.SignIn)
            {
                Console.WriteLine("\n\n\n\nUser Signed In\n\n\n\n\n");
                UserId = userId;
                UserFirstName = await UserService.GetUserNameFromId(userId);

                MessageService.AddMessage(new MessageBase("User Signed In", MessageBase.MessageType.Success));

                // TODO logging
            } else if(userEvent == UserEvent.SignOut)
            {
                Console.WriteLine("\n\n\nUser Signed Out\n\n\n");

                UserId = null;
                UserFirstName = null;


                MessageService.AddMessage(new MessageBase("Signed Out Successfully", MessageBase.MessageType.Success));
                // TODO logging
            }

            NullifyEmptyIds();

            OnUserChange?.Invoke(UserFirstName);

            IsUpToDate = true;
        }

        public string GetSignOutPage() => UserService.GetSignOutPage();

        public async Task Load()
        {
            if (IsUpToDate)
            {
                return;
            }

            UserId = await UserService.GetCurrentUserId();
            if(!UserId.IsNullOrEmpty())
            {
                UserFirstName = await UserService.GetUserNameFromId(UserId);
            } else
            {
                UserFirstName = null;
            }

            NullifyEmptyIds();
            OnUserChange?.Invoke(UserFirstName);

            IsUpToDate = true;
        }

        private void NullifyEmptyIds()
        {
            if (UserId == string.Empty)
            {
                UserId = null;
            }
            if (UserFirstName == string.Empty)
            {
                UserFirstName = null;
            }
        }

        public void Dispose()
        {
            UserService.RemoveUserListener(this);
        }
    }
}
