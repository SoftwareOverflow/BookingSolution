using Core;
using MudBlazor;

namespace Admin.Data.Helpers
{
    public class ViewServiceBase
    {
        private MessageManager Messages;

        public ViewServiceBase(MessageManager messageManager)
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
                    Messages.AddMessage(new MessageBase(error, Severity.Error));
                }
            }

            return default;
        }
    }
}
