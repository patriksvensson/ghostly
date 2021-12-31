using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core.Mvvm.Commands;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Services;
using Ghostly.Utilities;
using Ghostly.ViewModels.Dialogs;

namespace Ghostly.ViewModels.Commands
{
    public sealed class ExportProfileCommand : AsyncCommand<Account>
    {
        private readonly IDialogService _dialogs;
        private readonly IProfileService _profiles;
        private readonly IMessageService _messenger;
        private readonly INetworkHelper _network;
        private readonly ILocalizer _localizer;
        private readonly IReadOnlyList<IVendor> _vendors;

        public ExportProfileCommand(
            IDialogService dialogs,
            IProfileService profiles,
            IMessageService messenger,
            INetworkHelper network,
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
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _vendors = vendors.ToReadOnlyList();
        }

        protected override bool CanExecuteCommand(Account parameter)
        {
            return parameter?.ExportEnabled ?? false;
        }

        protected override async Task ExecuteCommand(Account account)
        {
            // Get the account vendor
            var vendor = _vendors.FirstOrDefault(x => x.Matches(account.VendorKind));
            if (vendor == null)
            {
                return;
            }

            // Define how we get the settings profiles
            Func<Task<IReadOnlyList<SettingsProfile>>> fetcher = () =>
            {
                return vendor.Profiles.GetProfiles(account);
            };

            // Show the profile selection dialog
            var result = await _dialogs.ShowDialog(new SelectProfileViewModel.Request(fetcher, true));
            if (result == null)
            {
                return;
            }

            var name = await GetProfileName(result);
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            // Get the profile we're going to sync with the Vendor
            var profile = await _profiles.Export(name);

            var success = true;
            using (var progress = new IndeterminateProgressReporter(_messenger))
            {
                await progress.ShowProgress(_localizer.Format("ExportProfile_Progress", name));
                success = await vendor.Profiles.Export(account, profile);
            }

            if (!success)
            {
                await _dialogs.ShowDialog(new MessageBoxViewModel.Request
                {
                    Glyph = "\uE783",
                    Title = _localizer.GetString("General_ErrorOccured"),
                    Body = _localizer.GetString("ExportProfile_Error"),
                    PrimaryText = _localizer.GetString("General_Ok"),
                });
            }
        }

        private async Task<string> GetProfileName(SelectProfileViewModel.Response result)
        {
            if (result.Create)
            {
                var existingProfileNames = result.AllProfiles.Select(x => x.Name);
                var profileName = _network.GetHostName();
                return await _dialogs.ShowDialog(new NewProfileViewModel.Request(existingProfileNames, profileName));
            }

            return result.Profile.Name;
        }
    }
}
