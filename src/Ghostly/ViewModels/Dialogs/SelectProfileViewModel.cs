using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Mvvm;
using Ghostly.Core.Services;
using Ghostly.Domain;

namespace Ghostly.ViewModels.Dialogs
{
    public sealed class SelectProfileViewModel
        : DialogScreen<SelectProfileViewModel.Request, SelectProfileViewModel.Response>, IValidatableDialog
    {
        private readonly ILocalizer _localizer;
        private readonly IGhostlyLog _log;
        private Request _request;

        public sealed class ProfileListViewItem
        {
            public string Text { get; set; }
            public string Glyph => Create ? Constants.Glyphs.Add : Constants.Glyphs.Profile;

            public SettingsProfile Profile { get; set; }
            public bool Create { get; set; }
        }

        public ObservableCollection<ProfileListViewItem> Profiles { get; }
        public Stateful<ProfileListViewItem> Selected { get; }
        public Stateful<bool> Loading { get; }
        public Stateful<bool> Loaded { get; }
        public Stateful<bool> ErrorLoading { get; }

        public string Header { get; set; }
        public string CreateText { get; set; }

        public sealed class Request : IDialogRequest<Response>
        {
            public bool AllowCreate { get; }
            public Func<Task<IReadOnlyList<SettingsProfile>>> ProfileFetcher { get; }

            public Request(
                Func<Task<IReadOnlyList<SettingsProfile>>> profileFetcher,
                bool allowCreate)
            {
                ProfileFetcher = profileFetcher;
                AllowCreate = allowCreate;
            }
        }

        public sealed class Response
        {
            public bool Create { get; set; }
            public SettingsProfile Profile { get; set; }
            public IReadOnlyList<SettingsProfile> AllProfiles { get; set; }
        }

        public SelectProfileViewModel(ILocalizer localizer, IGhostlyLog log)
        {
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            Profiles = new ObservableCollection<ProfileListViewItem>();
            Loading = new Stateful<bool>(false);
            Loaded = new Stateful<bool>(false);
            ErrorLoading = new Stateful<bool>(false);
            Selected = new Stateful<ProfileListViewItem>();
        }

        public override void Initialize(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            _request = request;

            // Initialize fields
            Header = _request.AllowCreate ? _localizer.GetString("SelectProfile_Title_Export") : _localizer.GetString("SelectProfile_Title_Import");
            CreateText = _request.AllowCreate ? _localizer.GetString("General_Export") : _localizer.GetString("General_Import");
        }

        public override async Task OnShown()
        {
            Loading.Value = true;

            try
            {
                var profiles = await _request.ProfileFetcher();
                if (profiles?.Count > 0)
                {
                    foreach (var profile in profiles)
                    {
                        Profiles.Add(new ProfileListViewItem
                        {
                            Profile = profile,
                            Text = profile.Name,
                        });
                    }
                }

                if (_request.AllowCreate)
                {
                    Profiles.Add(new ProfileListViewItem
                    {
                        Create = true,
                        Text = _localizer.GetString("SelectProfile_CreateNewProfile"),
                    });
                }

                Loaded.Value = true;
                ErrorLoading.Value = false;
            }
            catch (Exception exception)
            {
                ErrorLoading.Value = true;
                Loaded.Value = false;

                _log.Error(exception, "An error occured while retrieving account profiles.");
            }
            finally
            {
                Loading.Value = false;
            }
        }

        public bool Validate()
        {
            return Selected.Value != null;
        }

        public override Response GetResult(bool ok)
        {
            if (ok)
            {
                return new Response
                {
                    Create = _request.AllowCreate && (Selected.Value?.Create ?? false),
                    Profile = Selected.Value?.Profile,
                    AllProfiles = Profiles.Where(x => x.Profile != null).Select(x => x.Profile).ToReadOnlyList(),
                };
            }

            return null;
        }
    }
}
