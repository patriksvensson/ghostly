using System;
using System.Threading.Tasks;
using Ghostly.Core.Pal;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.ApplicationModel;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    public sealed class UwpPackageService : IPackageService
    {
        public string GetName()
        {
            return Package.Current.DisplayName;
        }

        public string GetVersion()
        {
            return Package.Current.Id.Version.FormatAsThreeComponents();
        }

        public string GetFirstInstalledVersion()
        {
            return SystemInformation.Instance.FirstVersionInstalled.FormatAsThreeComponents();
        }
    }
}
