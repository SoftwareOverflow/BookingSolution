using Core.Dto;
using Core.Interfaces;

namespace Core.Services
{
    internal class MessageService : IMessageService
    {
        private Action<MessageBase>? OnMessage;
        public void AddMessage(MessageBase message)
        {
            OnMessage?.Invoke(message);
        }

        public void AddMessageListener(Action<MessageBase> listener)
        {
            OnMessage += listener;
        }

        public void RemoveMessageListener(Action<MessageBase> listener)
        {
            OnMessage -= listener;
        }
    }
}
