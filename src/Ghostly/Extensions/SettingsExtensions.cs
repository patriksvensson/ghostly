using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;

namespace Ghostly
{
    public static class SettingsExtensions
    {
        public static bool GetShowNotificationToasts(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue("ShowNotificationsToasts", defaultValue: true);
        }

        public static void SetShowNotificationToasts(this ILocalSettings settings, bool value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue("ShowNotificationsToasts", value);
        }

        public static bool GetAggregateNotificationToasts(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue("AggregateNotificationToasts", defaultValue: false);
        }

        public static void SetAggregateNotificationToasts(this ILocalSettings settings, bool value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue("AggregateNotificationToasts", value);
        }

        public static Theme GetAppBackgroundRequestedTheme(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue<Theme>("AppBackgroundRequestedTheme");
        }

        public static void SetAppBackgroundRequestedTheme(this ILocalSettings settings, Theme theme)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue<Theme>("AppBackgroundRequestedTheme", theme);
        }

        public static bool GetAllowMeteredConnection(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue(Constants.Settings.AllowMeteredConnection, defaultValue: false);
        }

        public static void SetAllowMeteredConnection(this ILocalSettings settings, bool value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue(Constants.Settings.AllowMeteredConnection, value);
        }

        public static bool GetTelemetryEnabled(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue(Constants.Settings.TelemetryEnabled, defaultValue: true);
        }

        public static void SetTelemetryEnabled(this ILocalSettings settings, bool value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue(Constants.Settings.TelemetryEnabled, value);
        }

        public static bool GetSynchronizationEnabled(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue("SynchronizationEnabled", defaultValue: true);
        }

        public static void SetSynchronizationEnabled(this ILocalSettings settings, bool value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue("SynchronizationEnabled", value);
        }

        public static int GetSynchronizationInterval(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue("SynchronizationInterval", defaultValue: 10);
        }

        public static void SetSynchronizationInterval(this ILocalSettings settings, int value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue("SynchronizationInterval", value);
        }

        public static LogLevel GetLogLevel(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return (LogLevel)settings.GetValue("LogLevel", (int)LogLevel.Information);
        }

        public static void SetLogLevel(this ILocalSettings settings, LogLevel value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue("LogLevel", (int)value);
        }

        public static bool GetShowBadge(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue("ShowBadge", defaultValue: true);
        }

        public static void SetShowBadge(this ILocalSettings settings, bool value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue("ShowBadge", value);
        }

        public static bool GetScrollToLastComment(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue(Constants.Settings.ScrollToLastComment, defaultValue: true);
        }

        public static void SetScrollToLastComment(this ILocalSettings settings, bool value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue(Constants.Settings.ScrollToLastComment, value);
        }

        public static bool GetSetSynchronizeUnreadState(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue(Constants.Settings.SynchronizeUnreadState, defaultValue: true);
        }

        public static void SetSynchronizeUnreadState(this ILocalSettings settings, bool value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue(Constants.Settings.SynchronizeUnreadState, value);
        }

        public static GhostlyTheme GetTheme(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return (GhostlyTheme)settings.GetValue("Theme", (int)GhostlyTheme.Default);
        }

        public static void SetTheme(this ILocalSettings settings, GhostlyTheme theme)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue("Theme", (int)theme);
        }

        public static bool GetMarkMutedAsRead(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue("MarkMutedAsRead", defaultValue: false);
        }

        public static void SetMarkMutedAsRead(this ILocalSettings settings, bool value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue("MarkMutedAsRead", value);
        }

        public static bool GetAutomaticallyMarkNotificationsAsRead(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue(Constants.Settings.AutomaticallyMarkNotificationsAsRead, defaultValue: false);
        }

        public static void SetAutomaticallyMarkNotificationsAsRead(this ILocalSettings settings, bool value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue(Constants.Settings.AutomaticallyMarkNotificationsAsRead, value);
        }

        public static bool GetShowAvatars(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue(Constants.Settings.ShowAvatars, defaultValue: true);
        }

        public static void SetShowAvatars(this ILocalSettings settings, bool value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue(Constants.Settings.ShowAvatars, value);
        }

        public static bool GetPreferInternetAvatars(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue(Constants.Settings.PreferInternetAvatars, defaultValue: true);
        }

        public static void SetPreferInternetAvatars(this ILocalSettings settings, bool value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue(Constants.Settings.PreferInternetAvatars, value);
        }

        public static bool GetSynchronizeAvatars(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue(Constants.Settings.SynchronizeAvatars, defaultValue: true);
        }

        public static void SetSynchronizeAvatars(this ILocalSettings settings, bool value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            settings.SetValue(Constants.Settings.SynchronizeAvatars, value);
        }

        public static string GetCurrentCulture(this ILocalSettings settings)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return settings.GetValue<string>(Constants.Settings.CurrentCulture, defaultValue: null);
        }

        public static void SetCurrentCulture(this ILocalSettings settings, string value)
        {
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            if (value is null)
            {
                throw new System.ArgumentNullException(nameof(value));
            }

            settings.SetValue(Constants.Settings.CurrentCulture, value);
        }
    }
}
