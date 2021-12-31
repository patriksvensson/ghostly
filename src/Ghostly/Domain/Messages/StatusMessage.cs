namespace Ghostly.Domain.Messages
{
    public sealed class StatusMessage
    {
        public string Message { get; }
        public int Percentage { get; }

        public StatusMessage(string message, int? percentage)
        {
            Message = message ?? string.Empty;
            Percentage = percentage ?? -1;
        }
    }
}
