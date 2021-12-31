using System;

namespace Ghostly.Core.Mvvm.Events
{
    public sealed class SelectedItemChangedEventArgs : EventArgs
    {
        public object Old { get; }
        public object New { get; }

        public SelectedItemChangedEventArgs(object old, object @new)
        {
            Old = old;
            New = @new;
        }
    }
}
