using Windows.ApplicationModel;

namespace Ghostly.Uwp
{
    internal static class PackageVersionExtensions
    {
        public static string FormatAsThreeComponents(this PackageVersion version)
        {
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
