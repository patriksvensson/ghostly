namespace Ghostly.Core
{
    public static class ThemeExtensions
    {
        public static string GetName(this GhostlyTheme theme)
        {
            return theme.ToString();
        }

        public static string GetCanonicalName(this GhostlyTheme theme)
        {
            if (theme == GhostlyTheme.Light)
            {
                return "Light";
            }
            else
            {
                return "Dark";
            }
        }
    }
}
