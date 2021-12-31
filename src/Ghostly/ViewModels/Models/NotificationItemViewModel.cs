using Ghostly.Domain;

namespace Ghostly.ViewModels
{
    public sealed class NotificationViewModel
    {
        public Notification Notification { get; set; }
        public WorkItem WorkItem { get; set; }
        public string Html { get; set; }
    }
}
