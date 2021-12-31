using System;
using System.Linq.Expressions;
using Ghostly.Data.Models;

namespace Ghostly.Domain
{
    public sealed class Rule : Entity
    {
        private string _name;
        private bool _enabled;
        private string _expression;
        private string _description;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty(ref _enabled, value);
        }

        public string Expression
        {
            get => _expression;
            set => SetProperty(ref _expression, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public int SortOrder { get; set; }
        public bool Star { get; set; }
        public bool Mute { get; set; }
        public bool MarkAsRead { get; set; }
        public bool StopProcessing { get; set; }
        public Category Category { get; set; }

        public string ImportedFrom { get; set; }
        public DateTime? ImportedAt { get; set; }

        // Computed
        public Expression<Func<NotificationData, bool>> Filter { get; set; }
    }
}
