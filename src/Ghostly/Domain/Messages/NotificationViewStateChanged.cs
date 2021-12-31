namespace Ghostly.Domain.Messages
{
    public sealed class NotificationViewStateChanged
    {
        public int? CategoryId { get; }
        public bool Search { get; set; }

        // TODO: fix this...
        public NotificationViewStateChanged(int? categoryId, bool search = false)
        {
            CategoryId = categoryId;
            Search = search;
        }
    }
}
