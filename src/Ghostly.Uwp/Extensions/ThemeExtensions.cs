using Ghostly.Core;
using Windows.UI.Xaml;

namespace Ghostly.Uwp
{
    internal static class ThemeExtensions
    {
        public static ElementTheme ToElementTheme(this GhostlyTheme theme)
        {
            switch (theme)
            {
                case GhostlyTheme.Light:
                    return ElementTheme.Light;
                case GhostlyTheme.Dark:
                    return ElementTheme.Dark;
            }

            return ElementTheme.Default;
        }
    }
}
