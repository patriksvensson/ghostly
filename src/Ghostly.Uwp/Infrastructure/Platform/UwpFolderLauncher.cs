using System;
using System.Threading.Tasks;
using Ghostly.Core.Pal;
using Windows.Storage;
using Windows.System;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    public sealed class UwpFolderLauncher : IFolderLauncher
    {
        public async Task LaunchLogFolder()
        {
            var path = $"{ApplicationData.Current.LocalFolder.Path}\\Logs";
            var folder = await StorageFolder.GetFolderFromPathAsync(path);

            await Launcher.LaunchFolderAsync(folder);
        }
    }
}
