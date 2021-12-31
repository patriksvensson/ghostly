using Windows.ApplicationModel.Background;

namespace Ghostly.Uwp
{
    internal static class BackgroundAccessStatusExtensions
    {
        public static bool IsAllowed(this BackgroundAccessStatus accessStatus)
        {
            return accessStatus != BackgroundAccessStatus.DeniedBySystemPolicy &&
                accessStatus != BackgroundAccessStatus.DeniedByUser;
        }

        public static bool IsDisallowed(this BackgroundAccessStatus accessStatus)
        {
            return !IsAllowed(accessStatus);
        }
    }
}
