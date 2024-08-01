namespace Admin.Data.Helpers
{
    public class MessageManager
    {
        public Action<MessageBase>? OnMessage;

        public void AddMessage(MessageBase message) => OnMessage?.Invoke(message);
    }
}
