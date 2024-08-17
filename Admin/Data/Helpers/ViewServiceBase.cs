using Core.Dto;
using Core.Interfaces;
using Core.Responses;

namespace Admin.Data.Helpers
{
    public class ViewServiceBase
    {
        private IMessageService Messages;

        public ViewServiceBase(IMessageService messageManager)
        {
            Messages = messageManager;
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
                    Messages.AddMessage(new MessageBase(error, MessageBase.MessageType.Error));
                }
            }

            return default;
        }
    }
}
