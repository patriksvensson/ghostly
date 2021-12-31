namespace Ghostly.Domain
{
    public sealed class NotificationCategory : Entity
    {
        private string _name;
        private string _glyph;

        public string Glyph
        {
            get => _glyph;
            set => SetProperty(ref _glyph, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private bool _archive;
        public bool Archive
        {
            get => _archive;
            set => SetProperty(ref _archive, value);
        }
    }
}
