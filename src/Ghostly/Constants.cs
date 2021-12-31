using System;

namespace Ghostly
{
    public static class Constants
    {
        public static Uri GitHubIcon { get; } = new Uri("ms-appx:///Assets/Icons/github.svg");

        public static class Task
        {
            public static class InProc
            {
                public const string Sync = "Ghostly.InProc.Sync";
            }
        }

        public static class Templates
        {
            public const string Default = "Ghostly.Uwp.Assets.Templates.Notification.html";
            public const string GitHubVulnerability = "Ghostly.Uwp.Assets.Templates.Vulnerability.html";
            public const string GitHubDiscussion = "Ghostly.Uwp.Assets.Templates.Discussion.html";

            public static string[] GetAll()
            {
                return new[] { Default, GitHubVulnerability, GitHubDiscussion };
            }
        }

        public static class Glyphs
        {
            public const string Category = "\uF168";
            public const string Filter = "\uE71C";
            public const string Archive = "\uE7B8";
            public const string Inbox = "\uE715";
            public const string Add = "\uE710";
            public const string Profile = "\uE748";
        }

        public static class Emojis
        {
            public const string Inbox = "✉";
            public const string Archive = "🗃";
            public const string Muted = "🔇";
            public const string Starred = "⭐";
        }

        public static class TrackingEvents
        {
            public const string AppStarted = "AppStarted";
            public const string ExecutedRules = "ExecutedRules";
            public const string ExecutedSingleRule = "ExecutedSingleRule";
            public const string CreatedCategory = "CreatedCategory";
            public const string CreatedRule = "CreatedRule";
            public const string DeletedCategory = "DeletedCategory";
            public const string DeletedRule = "DeletedRule";
            public const string UpdatedCategory = "UpdatedCategory";
            public const string UpdatedRule = "UpdatedRule";

            public const string SignInEmbedded = "SignInEmbedded";
            public const string SignInBrowser = "SignInBrowser";
            public const string SignInCancelled = "SignInCancelled";
            public const string SignInFailed = "SignInFailed";
            public const string SignOut = "SignOut";
            public const string Authenticated = "Authenticated";

            public const string ExportedProfile = "ExportedProfile";
            public const string ImportedProfile = "ImportedProfile";
        }

        public static class Settings
        {
            public const string AllowMeteredConnection = "AllowMeteredConnection";
            public const string ScrollToLastComment = "ScrollToLastComment";
            public const string SynchronizeUnreadState = "SynchronizeUnreadState";
            public const string AutomaticallyMarkNotificationsAsRead = "AutomaticallyMarkNotificationsAsRead";
            public const string SynchronizeAvatars = "SynchronizeAvatars";
            public const string ShowAvatars = "ShowAvatars";
            public const string PreferInternetAvatars = "PreferInternetAvatars";
            public const string TelemetryEnabled = "TelemetryEnabled";
            public const string CurrentCulture = "CurrentCulture";
        }
    }
}
