using System.Threading.Tasks;
using Ghostly.Features.Synchronization;

namespace Ghostly.Domain
{
    public enum Vendor
    {
        Unknown = 0,
        GitHub = 1,
    }

    public abstract class Vendor<TAccountModel> : IVendor
        where TAccountModel : Account
    {
        public abstract IVendorProfiles Profiles { get; }

        public abstract Task OpenInBrowser(Account model);

        public abstract Task<bool> CanLogin();
        public abstract Task Login(Vendor vendor);
        public abstract Task Logout(Account model);

        public abstract bool Matches(Vendor vendor);

        public virtual bool CanSynchronize(TAccountModel model)
        {
            return true;
        }

        public abstract Task<bool> MarkAsRead(Notification notification);
        public abstract Task<SynchronizationStatus> Synchronize(TAccountModel model);
        public abstract Task<SynchronizationStatus> Synchronize(TAccountModel model, Notification notification);

        bool IVendor.CanSynchronize(Account model)
        {
            if (!(model is TAccountModel accountModel))
            {
                return false;
            }

            return CanSynchronize(accountModel);
        }

        Task<SynchronizationStatus> IVendor.Synchronize(Account model)
        {
            if (!(model is TAccountModel accountModel))
            {
                return Task.FromResult(SynchronizationStatus.UnknownError);
            }

            return Synchronize(accountModel);
        }

        public Task<SynchronizationStatus> Synchronize(Account model, Notification notification)
        {
            if (!(model is TAccountModel accountModel))
            {
                return Task.FromResult(SynchronizationStatus.UnknownError);
            }

            return Synchronize(accountModel, notification);
        }
    }
}
