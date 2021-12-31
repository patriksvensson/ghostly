using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Ghostly.Data.Models;

namespace Ghostly.Domain
{
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Data")]
    public abstract class Notification : Entity
    {
        private DateTime _timestamp;
        private bool _unread;
        private bool _muted;
        private bool _starred;

        public int AccountId { get; set; }
        public int WorkItemId { get; set; }

        public int CategoryId => Category?.Id ?? 0;
        public NotificationCategory Category { get; set; }

        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                _timestamp = value;
                NotifyPropertyChanged(nameof(Timestamp));
            }
        }

        public bool Unread
        {
            get => _unread;
            set
            {
                _unread = value;
                NotifyPropertyChanged(nameof(Unread));
                NotifyPropertyChanged(nameof(Read));
            }
        }

        public bool Muted
        {
            get => _muted;
            set
            {
                _muted = value;
                NotifyPropertyChanged(nameof(Muted));
            }
        }

        public bool Starred
        {
            get => _starred;
            set
            {
                _starred = value;
                NotifyPropertyChanged(nameof(Starred));
            }
        }

        // Dynamic
        public virtual string Template => Constants.Templates.Default;
        public virtual bool ShowExternalId => true;

        // Work item stuff
        public long ExternalId { get; set; }
        public WorkItemState State { get; set; }
        public string Title { get; set; }
        public string Preamble { get; set; }
        public string Origin { get; set; }
        public bool? Merged { get; set; }
        public bool? Draft { get; set; }
        public bool? Locked { get; set; }
        public Uri Url { get; set; }
        public List<Tag> Tags { get; set; }
        public Milestone Milestone { get; set; }

        // Calculated
        public bool Read => !Unread;
    }
}
