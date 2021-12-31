using Ghostly.Data.Models;
using Ghostly.Domain;

namespace Ghostly.Features.Activities.Payloads
{
    public sealed class MarkAsReadPayload : ActivityPayload
    {
        public Vendor Vendor { get; set; }
        public int NotificationId { get; set; }

        public override ActivityCategory Category => ActivityCategory.Synchronization;
        public override ActivityKind Kind => ActivityKind.MarkAsRead;
        public override ActivityConstraint Contstraint => ActivityConstraint.RequiresInternetConnection;
    }
}
