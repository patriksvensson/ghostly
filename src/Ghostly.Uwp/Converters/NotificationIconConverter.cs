using System;
using Ghostly.Data.Models;
using Ghostly.Domain.GitHub;
using MahApps.Metro.IconPacks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Ghostly.Uwp.Converters
{
    public sealed class NotificationIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is GitHubNotification notification))
            {
                throw new InvalidOperationException("Not a notification.");
            }

            if (!(parameter is string parameterString) || string.IsNullOrWhiteSpace(parameterString))
            {
                throw new InvalidOperationException("No parameter specified.");
            }

            if (parameterString == "Icon")
            {
                if (notification.Kind == GitHubWorkItemKind.Issue)
                {
                    if (notification.State == WorkItemState.Open)
                    {
                        return PackIconOcticonsKind.IssueOpened;
                    }
                    else if (notification.State == WorkItemState.Closed)
                    {
                        return PackIconOcticonsKind.IssueClosed;
                    }
                    else
                    {
                        return PackIconOcticonsKind.IssueReopened;
                    }
                }
                else if (notification.Kind == GitHubWorkItemKind.PullRequest)
                {
                    if (notification.Merged != null && notification.Merged.Value)
                    {
                        return PackIconOcticonsKind.GitMerge;
                    }

                    return PackIconOcticonsKind.GitPullRequest;
                }
                else if (notification.Kind == GitHubWorkItemKind.Release)
                {
                    return PackIconOcticonsKind.Tag;
                }
                else if (notification.Kind == GitHubWorkItemKind.Vulnerability)
                {
                    return PackIconOcticonsKind.Alert;
                }
                else if (notification.Kind == GitHubWorkItemKind.Commit)
                {
                    return PackIconOcticonsKind.GitCommit;
                }
                else if (notification.Kind == GitHubWorkItemKind.Discussion)
                {
                    return PackIconOcticonsKind.CommentDiscussion;
                }
                else
                {
                    throw new NotImplementedException("Unknown notification kind.");
                }
            }
            else if (parameterString == "Foreground")
            {
                if (notification.Kind == GitHubWorkItemKind.Issue)
                {
                    if (notification.State == WorkItemState.Open)
                    {
                        return (Brush)Application.Current.Resources["GitHubIssueOpenBrush"];
                    }
                    else if (notification.State == WorkItemState.Closed)
                    {
                        return (Brush)Application.Current.Resources["GitHubIssueClosedBrush"];
                    }
                    else
                    {
                        return (Brush)Application.Current.Resources["GitHubIssueReopenedBrush"];
                    }
                }
                else if (notification.Kind == GitHubWorkItemKind.PullRequest)
                {
                    if (notification.Merged != null && notification.Merged.Value)
                    {
                        return (Brush)Application.Current.Resources["GitHubPullRequestMergedBrush"];
                    }

                    if (notification.State == WorkItemState.Open)
                    {
                        // Draft PR?
                        if (notification.Draft != null && notification.Draft.Value)
                        {
                            return (Brush)Application.Current.Resources["GitHubPullRequestDraftBrush"];
                        }
                        else
                        {
                            return (Brush)Application.Current.Resources["GitHubPullRequestOpenBrush"];
                        }
                    }
                    else
                    {
                        return (Brush)Application.Current.Resources["GitHubPullRequestClosedBrush"];
                    }
                }
                else if (notification.Kind == GitHubWorkItemKind.Release)
                {
                    return (Brush)Application.Current.Resources["GitHubReleaseBrush"];
                }
                else if (notification.Kind == GitHubWorkItemKind.Vulnerability)
                {
                    return (Brush)Application.Current.Resources["GitHubVulnerabilityBrush"];
                }
                else if (notification.Kind == GitHubWorkItemKind.Commit)
                {
                    return (Brush)Application.Current.Resources["GitHubCommitBrush"];
                }
                else if (notification.Kind == GitHubWorkItemKind.Discussion)
                {
                    return (Brush)Application.Current.Resources["GitHubCommitBrush"];
                }
            }

            throw new InvalidOperationException("Unknown parameter");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
