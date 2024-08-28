using Auth.Interfaces;
using Core.Dto;
using Core.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Core.Services
{
    internal class UserService : IUserObserver, IDisposable, Interfaces.IUserService, IUserServiceInternal
    {
        private Auth.Interfaces.IUserService AuthUserService { get; set; }

        private IMessageService MessageService { get; set; }

        /// <summary>
        /// Determines if the UserStateManager has up to date information.
        /// Call <seealso cref="UserService.Load"/> Load if this is false and up to date information is required
        /// </summary>
        public bool IsUpToDate { get; private set; } = false;

        private string UserId = "";

        private string UserName = "";

        public UserService(Auth.Interfaces.IUserService userService, IMessageService messageService)
        {
            AuthUserService = userService;
            AuthUserService.AddUserListener(this);

            MessageService = messageService;
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
            }
            else if (userEvent == UserEvent.SignOut)
            {
                userId = string.Empty;

                MessageService.AddMessage(new MessageBase("Signed Out Successfully", MessageBase.MessageType.Success));
                // TODO logging
            }

            UserId = userId;
            UserName = name;

            IsUpToDate = true;
        }

        public string GetSignOutPage() => AuthUserService.GetSignOutPage();

        private async Task Load()
        {
            if (IsUpToDate)
            {
                return;
            }

            var id = await AuthUserService.GetCurrentUserIdAsync();
            var name = string.Empty;
            if (!id.IsNullOrEmpty())
            {
                name = await AuthUserService.GetUserNameFromId(id);
            }

            UserId = id;
            UserName = name;

            IsUpToDate = true;
        }

        public async Task<string?> GetUserNameAsync()
        {
            if (!IsUpToDate)
            {
                await Load();
            }

            return UserName;
        }

        public async Task<string> GetUserIdAsync()
        {
            if(!IsUpToDate)
            {
                await Load();
            }

            return UserId;
        }

        public void Dispose()
        {
            AuthUserService.RemoveUserListener(this);
        }
    }
}
