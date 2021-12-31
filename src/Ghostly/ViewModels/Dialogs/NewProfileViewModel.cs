using System;
using System.Collections.Generic;
using Ghostly.Core.Mvvm;

namespace Ghostly.ViewModels.Dialogs
{
    public sealed class NewProfileViewModel
        : DialogScreen<NewProfileViewModel.Request, string>, IValidatableDialog
    {
        public Stateful<string> ProfileName { get; set; }
        public Stateful<bool> IsValid { get; set; }
        public HashSet<string> ExistingProfileNames { get; }

        public sealed class Request : IDialogRequest<string>
        {
            public string ProfileName { get; }
            public List<string> ExistingProfileNames { get; }

            public Request(IEnumerable<string> existingProfileNames, string profileName = null)
            {
                ExistingProfileNames = new List<string>(existingProfileNames ?? Array.Empty<string>());
                ProfileName = profileName;
            }
        }

        public NewProfileViewModel()
        {
            IsValid = new Stateful<bool>(false);
            ProfileName = new Stateful<string>(text =>
            {
                IsValid.Value = Validate();
            });
            ExistingProfileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public override void Initialize(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ExistingProfileNames.Clear();
            ExistingProfileNames.AddRange(request.ExistingProfileNames);
            ProfileName.Value = request.ProfileName;
        }

        public bool Validate()
        {
            return !string.IsNullOrEmpty(ProfileName.Value) &&
                !ExistingProfileNames.Contains(ProfileName.Value);
        }

        public override string GetResult(bool ok)
        {
            if (ok)
            {
                return ProfileName.Value;
            }

            return null;
        }
    }
}
