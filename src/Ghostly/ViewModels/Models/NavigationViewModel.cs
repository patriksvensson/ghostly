using System;
using System.Collections.Generic;
using System.Linq;
using Ghostly.Core.Mvvm;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Services;

namespace Ghostly.ViewModels
{
    public class NavigationViewModel
    {
        private readonly IUnreadService _unread;
        private readonly ILocalizer _localizer;

        public NavigationItemCollection Categories { get; }
        public NavigationItemCollection Commands { get; }
        public NavigationItemCollection Menu { get; }

        public NavigationViewModel(
            IUnreadService unread, ILocalizer localizer,
            Action<NavigationItem> callback)
        {
            _unread = unread ?? throw new ArgumentNullException(nameof(unread));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));

            Categories = new NavigationItemCollection(callback);
            Commands = new NavigationItemCollection(callback);
            Menu = new NavigationItemCollection(callback);
        }

        public void Initialize(IEnumerable<Category> categories)
        {
            foreach (var category in categories.OrderBy(x => x.SortOrder))
            {
                _unread.UnreadCount.TryGetValue(category.Id, out var count);

                Categories.Add(new NavigationItem
                {
                    Id = category.Id,
                    Category = category,
                    IsDeletable = category.Deletable,
                    Count = count,
                    Name = category.Name,
                    Kind = NavigationItemKind.Category,
                    Glyph = category.Glyph,
                    Emoji = category.Emoji,
                    Muted = category.Muted,
                });
            }

            Commands.Add(new NavigationItem { Name = _localizer.GetString("Shell_NewCategory"), Kind = NavigationItemKind.NewCategory, Glyph = "\uE8F4" });

            Menu.Add(new NavigationItem { Name = _localizer.GetString("Shell_Search"), Kind = NavigationItemKind.Search, Glyph = "\uE721" });
            Menu.Add(new NavigationItem { Name = _localizer.GetString("Shell_Rules"), Kind = NavigationItemKind.Rules, Glyph = "\uE7C1" });
            Menu.Add(new NavigationItem { Name = _localizer.GetString("Shell_Accounts"), Kind = NavigationItemKind.Account, Glyph = "\uE77B" });
            Menu.Add(new NavigationItem { Name = _localizer.GetString("Shell_Settings"), Kind = NavigationItemKind.Settings, Glyph = "\uE713" });
        }

        public NavigationItem GetInbox()
        {
            return Categories.Single(c => c.Category.Inbox);
        }
    }
}