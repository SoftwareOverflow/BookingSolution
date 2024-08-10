using Core.Dto;

namespace Core.Interfaces
{
    public interface IMessageService
    {
        public void AddMessageListener(Action<MessageBase> listener);

        public void RemoveMessageListener(Action<MessageBase> listener);

        public void AddMessage(MessageBase message);
    }
}
