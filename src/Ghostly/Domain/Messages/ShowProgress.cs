namespace Ghostly.Domain.Messages
{
    public sealed class ShowProgress
    {
        public string Message { get; }
        public int Percentage { get; }

        public ShowProgress(string message, int? percentage)
        {
            Message = message ?? string.Empty;
            Percentage = percentage ?? -1;
        }
    }
}
