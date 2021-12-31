using System.Threading;

namespace Ghostly
{
    public static class GhostlyState
    {
        private static readonly AsyncLocal<bool> _isBackgroundActivated;

        public static bool InForeground { get; set; }

        public static bool IsBackgroundActivated
        {
            get => _isBackgroundActivated.Value;
            set => _isBackgroundActivated.Value = value;
        }

        static GhostlyState()
        {
            _isBackgroundActivated = new AsyncLocal<bool>();
            InForeground = false;
        }
    }
}
