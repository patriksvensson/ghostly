using System;
using Ghostly.Data.Models;

namespace Ghostly.Domain
{
    public abstract class Account : Entity
    {
        public abstract Vendor VendorKind { get; }
        public abstract string DisplayName { get; }
        public abstract string Description { get; }
        public abstract Uri Icon { get; }

        public abstract bool ImportEnabled { get; }
        public abstract bool ExportEnabled { get; }
        public bool ImportOrExportEnabled => ImportEnabled || ExportEnabled;

        private AccountState _state;
        public AccountState State
        {
            get => _state;
            set
            {
                SetProperty(ref _state, value);
                NotifyPropertyChanged(nameof(HasAuthorizationError));
            }
        }

        public DateTime? LastSyncedAt { get; set; }

        public bool HasAuthorizationError
        {
            get => State == AccountState.Unauthorized;
        }
    }
}
