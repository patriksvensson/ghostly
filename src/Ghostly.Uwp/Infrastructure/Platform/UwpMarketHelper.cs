using Ghostly.Core.Pal;
using Microsoft.Toolkit.Uwp.Helpers;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    public sealed class UwpMarketHelper : IMarketHelper
    {
        public bool IsFirstRun()
        {
            return SystemInformation.Instance.IsFirstRun;
        }

        public bool IsUpdated()
        {
            return SystemInformation.Instance.IsAppUpdated;
        }
    }
}
