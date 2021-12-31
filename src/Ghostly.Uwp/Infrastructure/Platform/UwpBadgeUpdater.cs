using System.Globalization;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    public sealed class UwpBadgeUpdater : IBadgeUpdater
    {
        private readonly ILocalSettings _settings;
        private readonly IGhostlyLog _log;
        private int _last;

        public UwpBadgeUpdater(
            ILocalSettings settings,
            IGhostlyLog log)
        {
            _settings = settings;
            _log = log;
        }

        public void Refresh()
        {
            Update(_last);
        }

        public void Update(int unread)
        {
            _last = unread;

            var showBadge = _settings.GetShowBadge();
            if (!showBadge)
            {
                _log.Debug("Can't update badge since it's turned off ({BadgeCount}).", unread);
                ClearBadge();
            }
            else
            {
                if (unread > 0)
                {
                    SetBadge(unread);
                }
                else
                {
                    ClearBadge();
                }
            }
        }

        private void SetBadge(int number)
        {
            _log.Debug("Updating Windows badge ({BadgeCount})...", number);

            // Create the badge XML.
            var badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            var badgeElement = badgeXml.SelectSingleNode("/badge") as XmlElement;
            badgeElement.SetAttribute("value", number.ToString(CultureInfo.InvariantCulture));

            // Update the badge.
            var badge = new BadgeNotification(badgeXml);
            var badgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();

            badgeUpdater.Update(badge);

            _log.Debug("Windows badge updated.");
        }

        private void ClearBadge()
        {
            _log.Debug("Clearing Windows badge...");
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
            _log.Debug("Windows badge cleared.");
        }
    }
}
