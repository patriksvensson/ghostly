using System.Threading.Tasks;
using Ghostly.Features.Synchronization;

namespace Ghostly.Domain
{
    public interface IVendor
    {
        // Settings
        IVendorProfiles Profiles { get; }

        // Authentication
        Task<bool> CanLogin();
        Task Login(Vendor vendor);
        Task Logout(Account model);

        // Synchronization
        bool CanSynchronize(Account model);
        Task<SynchronizationStatus> Synchronize(Account model);
        Task<SynchronizationStatus> Synchronize(Account model, Notification notification);

        // Misc
        bool Matches(Vendor vendorKind);
        Task OpenInBrowser(Account model);
        Task<bool> MarkAsRead(Notification notification);
    }
}
