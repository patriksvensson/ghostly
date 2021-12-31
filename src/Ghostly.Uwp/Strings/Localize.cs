using System.Diagnostics;
using System.Globalization;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Markup;

namespace Ghostly.Uwp.Strings
{
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    public sealed class Localize : MarkupExtension
    {
        public string Key { get; set; }

        protected override object ProvideValue()
        {
            if (!string.IsNullOrWhiteSpace(Key))
            {
                var result = GetString(Key);
                if (string.IsNullOrWhiteSpace(result))
                {
#if DEBUG
                    Debug.WriteLine(Key);
#endif
                    return Key;
                }

                return result;
            }

            return "[No_Localization_Key]";
        }

        public static string GetString(string key)
        {
            var result = ResourceLoader.GetForViewIndependentUse().GetString(key);
            if (string.IsNullOrWhiteSpace(result))
            {
#if DEBUG
                Debug.WriteLine(key);
#endif
                result = key;
            }

            return result;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, GetString(key), args);
        }
    }
}
