using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Services;
using Windows.Globalization;
using Culture = Ghostly.Domain.Culture;

namespace Ghostly.Uwp.Infrastructure
{
    public sealed class CultureService : ICultureService, IInitializable
    {
        private readonly List<Culture> _cultures;
        private readonly ILocalSettings _settings;
        private readonly IGhostlyLog _log;

        public Culture Current { get; private set; }

        public CultureService(ILocalSettings settings, IGhostlyLog log)
        {
            _cultures = new List<Culture>();
            _settings = settings;
            _log = log;
        }

        public IReadOnlyList<Culture> GetSupportedCultures()
        {
            return _cultures;
        }

        public Task<bool> Initialize(bool background)
        {
            // Add known cultures.
            CreateCulture("en-US");
            CreateCulture("en-GB");
            CreateCulture("sv-SE");
            CreateCulture("zh-CN");

            // Got a preferred culture?
            var preferredCultureName = _settings.GetCurrentCulture();
            if (!string.IsNullOrWhiteSpace(preferredCultureName))
            {
                _log.Debug("Preferred culture is {CultureName}.", preferredCultureName);
                var preferredCulture = _cultures.SingleOrDefault(l => l.CultureCode.Equals(preferredCultureName, StringComparison.OrdinalIgnoreCase));
                if (preferredCulture != null)
                {
                    if (TrySetLanguage(preferredCulture))
                    {
                        Current = preferredCulture;
                    }
                }
                else
                {
                    _log.Warning("Preferred culture ({CultureName}) is not supported by Ghostly.", preferredCultureName);
                }
            }
            else
            {
                // Is the current UI culture something we support?
                var systemCultureName = CultureInfo.CurrentCulture.Name;
                var systemCulture = _cultures.SingleOrDefault(l => l.CultureCode.Equals(systemCultureName, StringComparison.OrdinalIgnoreCase));
                if (systemCulture != null)
                {
                    if (TrySetLanguage(systemCulture))
                    {
                        Current = systemCulture;
                    }
                }
                else
                {
                    _log.Warning("System culture ({CultureName}) is not supported by Ghostly.", systemCultureName);
                }
            }

            // We're using the default culture.
            if (Current == null)
            {
                _log.Debug("No culture was decided on. Defaulting to {CultureName}.", "en-US");
                Current = _cultures.Single(l => l.CultureCode.Equals("en-us", StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult(true);
        }

        public void SetCulture(Culture language)
        {
            if (language is null)
            {
                throw new ArgumentNullException(nameof(language));
            }

            _settings.SetCurrentCulture(language.CultureCode);
        }

        private bool TrySetLanguage(Culture culture)
        {
            if (culture == null)
            {
                _log.Error("Can't set active culture since no culture code was provided.");
                return false;
            }

            if (!culture.CultureCode.Equals(ApplicationLanguages.PrimaryLanguageOverride, StringComparison.OrdinalIgnoreCase))
            {
                // Update the primary language override.
                _log.Information("Setting active culture to {CultureName} (was {PreviousCultureName}).", culture.CultureCode, ApplicationLanguages.PrimaryLanguageOverride);
                ApplicationLanguages.PrimaryLanguageOverride = culture.CultureCode;
            }
            else
            {
                _log.Debug("Active culture is {CultureName}.", ApplicationLanguages.PrimaryLanguageOverride);
            }

            return true;
        }

        private void CreateCulture(string cultureCode)
        {
            try
            {
                var culture = new CultureInfo(cultureCode);
                _cultures.Add(new Culture(
                    culture.TextInfo.ToTitleCase(culture.NativeName),
                    culture.Name,
                    culture.TwoLetterISOLanguageName));
            }
            catch
            {
                _log.Error("Could not create culture representation for {CultureName}.", cultureCode);
            }
        }
    }
}
