using System;

namespace Ghostly.Core.Mvvm
{
    // TODO: Divide into two kind of events?
    public sealed class NavigationItemInvokedEventArgs : EventArgs
    {
        public NavigationItemKind Kind { get; }
        public int? Id { get; }
        public bool Programatically { get; }

        public NavigationItemInvokedEventArgs(NavigationItemKind kind, bool programatically = false)
        {
            if (kind == NavigationItemKind.Category)
            {
                throw new InvalidOperationException("No ID provided for Category.");
            }

            Kind = kind;
            Programatically = programatically;
        }

        public NavigationItemInvokedEventArgs(NavigationItemKind kind, int id, bool programatically = false)
        {
            Kind = kind;
            Id = id;
            Programatically = programatically;
        }
    }
}
