namespace Ghostly.Core.Pal
{
    public interface IBadgeUpdater
    {
        void Refresh();
        void Update(int unread);
    }
}
