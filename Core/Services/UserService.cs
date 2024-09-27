using Auth.Interfaces;
using Core.Dto;
using Core.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Core.Services
{
    internal class UserService : IUserObserver, IDisposable, Interfaces.IUserService, IUserServiceInternal
    {
        private readonly Auth.Interfaces.IUserService _authUserService;

        private readonly IMessageService _messageService;

        /// <summary>
        /// Determines if the UserStateManager has up to date information.
        /// Call <seealso cref="UserService.Load"/> Load if this is false and up to date information is required
        /// </summary>
        private bool _isUpToDate = false;

        private string _userId = "";

        private string _userName = "";

        public UserService(Auth.Interfaces.IUserService userService, IMessageService messageService)
        {
            _authUserService = userService;
            _authUserService.AddUserListener(this);

            _messageService = messageService;
        }

        public async void OnUserEvent(UserEvent userEvent, string userId)
        {
            // TODO handle business logic here
            // TODO send any required messages using the MessageService
            // TODO remove complex tasks to a separate async Task<T> method to allow for better error handling. Errors thrown in async void will NOT be caught at higher levels

            var name = string.Empty;

            if (userEvent == UserEvent.Created)
            {

                name = await _authUserService.GetUserNameFromId(userId);

                // TODO create BusinessUser in db
                // TODO logging
                _messageService.AddMessage(new MessageBase("User Created Successfully", MessageBase.MessageType.Success));
            }
            else if (userEvent == UserEvent.SignIn)
            {
                name = await _authUserService.GetUserNameFromId(userId);

                _messageService.AddMessage(new MessageBase("User Signed In", MessageBase.MessageType.Success));

                // TODO logging
            }
            else if (userEvent == UserEvent.SignOut)
            {
                userId = string.Empty;

                _messageService.AddMessage(new MessageBase("Signed Out Successfully", MessageBase.MessageType.Success));
                // TODO logging
            }

            _userId = userId;
            _userName = name;

            _isUpToDate = true;
        }

        public string GetSignOutPage() => _authUserService.GetSignOutPage();

        private async Task Load()
        {
            if (_isUpToDate)
            {
                return;
            }

            var id = await _authUserService.GetCurrentUserIdAsync();
            var name = string.Empty;
            if (!id.IsNullOrEmpty())
            {
                name = await _authUserService.GetUserNameFromId(id);
            }

            _userId = id;
            _userName = name;

            _isUpToDate = true;
        }

        public async Task<string?> GetUserNameAsync()
        {
            if (!_isUpToDate)
            {
                await Load();
            }

            return _userName;
        }

        public async Task<string> GetUserIdAsync()
        {
            if(!_isUpToDate)
            {
                await Load();
            }

            return _userId;
        }

        public void Dispose()
        {
            _authUserService.RemoveUserListener(this);
        }
    }
}
