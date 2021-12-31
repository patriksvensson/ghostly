using System.Globalization;
using Ghostly.Core.Services;

namespace Ghostly
{
    public static class LocalizationServiceExtensions
    {
        public static string Format(this ILocalizer localization, string key, params object[] args)
        {
            if (localization is null)
            {
                throw new System.ArgumentNullException(nameof(localization));
            }

            return string.Format(CultureInfo.InvariantCulture, localization.GetString(key), args);
        }

        public static string GetString(this ILocalizer localization, bool condition, string @true, string @false)
        {
            if (localization is null)
            {
                throw new System.ArgumentNullException(nameof(localization));
            }

            return localization.GetString(condition ? @true : @false);
        }
    }
}
