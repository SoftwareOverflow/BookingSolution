using Core.Dto;
using Core.Interfaces;
using Core.Responses;

namespace Admin.Data.Helpers
{
    public class ViewServiceBase
    {
        private IMessageService _messageService;

        public ViewServiceBase(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task<T?> HandleServiceRequest<T>(Func<Task<ServiceResult<T>>> request)
        {
            var result = await request.Invoke();

            if (result.IsSuccess)
            {
                return result.Result;
            } else
            {
                foreach (var error in result.Errors)
                {
                    _messageService.AddMessage(new MessageBase(error, MessageBase.MessageType.Error));
                }
            }

            return default;
        }
    }
}
