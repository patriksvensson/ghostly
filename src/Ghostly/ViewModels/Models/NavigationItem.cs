using System;
using Ghostly.Core.Mvvm;
using Ghostly.Domain;

namespace Ghostly.ViewModels
{
    public class NavigationItem : Observable
    {
        private string _name;
        private bool _hasProblem;
        private string _glyph;
        private string _emoji;
        private bool _muted;

        public int? Id { get; set; }
        public NavigationItemKind Kind { get; set; }
        public Category Category { get; set; }
        public bool IsDeletable { get; set; }

        public bool IsCategory => Kind == NavigationItemKind.Category || Kind == NavigationItemKind.Inbox;
        public bool IsCommand => Kind == NavigationItemKind.NewCategory;
        public bool IsMenu => !IsCategory && !IsCommand;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool HasProblem
        {
            get => _hasProblem;
            set => SetProperty(ref _hasProblem, value);
        }

        public string Glyph
        {
            get => _glyph;
            set => SetProperty(ref _glyph, value);
        }

        public string Emoji
        {
            get => _emoji;
            set => SetProperty(ref _emoji, value);
        }

        public bool Muted
        {
            get => _muted;
            set => SetProperty(ref _muted, value);
        }

        private int _count;
        public int Count
        {
            get => _count;
            set
            {
                SetProperty(ref _count, value);
                NotifyPropertyChanged(nameof(CappedCount));
                NotifyPropertyChanged(nameof(ShowIndicator));
            }
        }

        public int CappedCount
        {
            get => Math.Min(_count, 99);
        }

        public bool ShowIndicator => Count > 0;

        public void Refresh()
        {
            NotifyPropertyChanged(nameof(Kind));
        }
    }
}