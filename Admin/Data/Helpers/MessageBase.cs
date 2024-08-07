using MudBlazor;

namespace Admin.Data.Helpers
{
    public class MessageBase
    {
        public string Message;

        public Severity Severity;

        public MessageBase(string message, Severity severity)
        {
            Message = message;
            Severity = severity;
        }
    }
}
