using System;
using System.Threading.Tasks;
using Ghostly.Core.Pal;
using Windows.System;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    public sealed class UwpUriLauncher : IUriLauncher
    {
        public async Task Launch(Uri uri)
        {
            await Launcher.LaunchUriAsync(uri);
        }
    }
}
