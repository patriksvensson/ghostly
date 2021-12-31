using System;
using Ghostly.Domain.Messages;

namespace Ghostly.Domain
{
    public static class NotificationExtensions
    {
        /// <summary>
        /// Determines whether or not the provided notification model. is in the specified state.
        /// </summary>
        /// <param name="model">The notification model.</param>
        /// <param name="state">The state.</param>
        /// <returns>Whether or not the notification is in the specified state.</returns>
        public static bool IsInState(this Notification model, UpdateNotificationState state)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            switch (state)
            {
                case UpdateNotificationState.Star:
                    return model.Starred;
                case UpdateNotificationState.Unstar:
                    return !model.Starred;
                case UpdateNotificationState.Mute:
                    return model.Muted;
                case UpdateNotificationState.Unmute:
                    return !model.Muted;
            }

            throw new InvalidOperationException("Unknown notification state.");
        }

        /// <summary>
        /// Updates the notification model with values from the provided source.
        /// </summary>
        /// <param name="model">The model to update.</param>
        /// <param name="source">The model with new values.</param>
        public static void Update(this Notification model, Notification source)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            model.Unread = source.Unread;
            model.Starred = source.Starred;
            model.Timestamp = source.Timestamp;
            model.State = source.State;
            model.Muted = source.Muted;
        }
    }
}
