using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core.Mvvm.Commands;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Services;
using Ghostly.Utilities;
using Ghostly.ViewModels.Dialogs;

namespace Ghostly.ViewModels.Commands
{
    public sealed class ImportProfileCommand : AsyncCommand<Account>
    {
        private readonly IDialogService _dialogs;
        private readonly IProfileService _profiles;
        private readonly IMessageService _messenger;
        private readonly ILocalizer _localizer;
        private readonly IReadOnlyList<IVendor> _vendors;

        public ImportProfileCommand(
            IDialogService dialogs,
            IProfileService profiles,
            IMessageService messenger,
            ILocalizer localizer,
            IEnumerable<IVendor> vendors)
        {
            if (vendors is null)
            {
                throw new ArgumentNullException(nameof(vendors));
            }

            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _vendors = vendors.ToReadOnlyList();
        }

        protected override bool CanExecuteCommand(Account parameter)
        {
            return parameter?.ImportEnabled ?? false;
        }

        protected override async Task ExecuteCommand(Account account)
        {
            // Get the account vendor
            var vendor = _vendors.FirstOrDefault(x => x.Matches(account.VendorKind));
            if (vendor == null)
            {
                return;
            }

            // Define how we get the settings profiles.
            Task<IReadOnlyList<SettingsProfile>> Fetcher() => vendor.Profiles.GetProfiles(account);

            // Show the profile selection dialog.
            var result = await _dialogs.ShowDialog(new SelectProfileViewModel.Request(Fetcher, false));
            if (result?.Profile != null)
            {
                using (var progress = new IndeterminateProgressReporter(_messenger))
                {
                    await progress.ShowProgress(_localizer.Format("ImportProfile_Progress", result.Profile.Name));
                    await _profiles.Import(result.Profile);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }
    }
}
