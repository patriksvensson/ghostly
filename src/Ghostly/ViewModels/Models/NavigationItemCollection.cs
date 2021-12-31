using System;
using System.Collections.ObjectModel;
using System.Linq;
using Ghostly.Core.Mvvm;

namespace Ghostly.ViewModels
{
    public class NavigationItemCollection : ObservableCollection<NavigationItem>
    {
        public Stateful<NavigationItem> Selected { get; }

        public NavigationItemCollection(Action<NavigationItem> callback = null)
        {
            Selected = new Stateful<NavigationItem>(callback ?? ((_) => { }));
        }

        public NavigationItem Find(NavigationItemKind kind)
        {
            return Items.SingleOrDefault(c => c.Kind == kind);
        }

        public NavigationItem Find(int id)
        {
            return Items.SingleOrDefault(c => c.Id == id);
        }
    }
}