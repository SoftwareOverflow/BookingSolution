using Core.Dto;
using Core.Interfaces;

namespace Core.Services
{
    internal class MessageService : IMessageService
    {
        private static readonly List<MessageBase> MessageCache = [];

        private Action<MessageBase>? OnMessage;
        public void AddMessage(MessageBase message)
        {
            if (OnMessage != null)
            {
                OnMessage?.Invoke(message);
            } else
            {
                MessageCache.Add(message);
            }
            
        }

        public void AddMessageListener(Action<MessageBase> listener)
        {
            OnMessage -= listener;
            OnMessage += listener;

            // Send any unactioned messages
            for (int i = MessageCache.Count - 1; i >= 0; i--)
            {
                var message = MessageCache[i];
                listener.Invoke(message);
                MessageCache.RemoveAt(i);
            }
        }

        public void RemoveMessageListener(Action<MessageBase> listener)
        {
            OnMessage -= listener;
        }
    }
}
