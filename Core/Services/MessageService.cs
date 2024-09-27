using Core.Dto;
using Core.Interfaces;

namespace Core.Services
{
    internal class MessageService : IMessageService
    {
        private static readonly List<MessageBase> _messageCahce = [];

        private Action<MessageBase>? _onMessage;
        public void AddMessage(MessageBase message)
        {
            if (_onMessage != null)
            {
                _onMessage?.Invoke(message);
            } else
            {
                _messageCahce.Add(message);
            }
            
        }

        public void AddMessageListener(Action<MessageBase> listener)
        {
            _onMessage -= listener;
            _onMessage += listener;

            // Send any unactioned messages
            for (int i = _messageCahce.Count - 1; i >= 0; i--)
            {
                var message = _messageCahce[i];
                listener.Invoke(message);
                _messageCahce.RemoveAt(i);
            }
        }

        public void RemoveMessageListener(Action<MessageBase> listener)
        {
            _onMessage -= listener;
        }
    }
}
