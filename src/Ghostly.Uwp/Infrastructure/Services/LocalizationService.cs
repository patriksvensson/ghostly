using System;
using Ghostly.Core.Services;
using Ghostly.Uwp.Strings;

namespace Ghostly.Uwp.Infrastructure
{
    public sealed class LocalizationService : ILocalizer
    {
        public string this[string key] => GetString(key);

        public string GetString(string key)
        {
            return Localize.GetString(key)?.Replace("\\n", Environment.NewLine, StringComparison.OrdinalIgnoreCase);
        }
    }
}
