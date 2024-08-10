namespace Core.Dto
{
    public class MessageBase
    {
        public string Message;

        public MessageType Severity;

        public MessageBase(string message, MessageType severity)
        {
            Message = message;
            Severity = severity;
        }

        public enum MessageType
        {
            Success, Normal, Warning, Error
        }
    }
}
