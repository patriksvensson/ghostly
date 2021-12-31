using System;
using System.Threading.Tasks;
using Ghostly.Core.Pal;
using Windows.ApplicationModel.Core;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    public sealed class UwpThreadingModel : IThreadingModel
    {
        public async Task ExecuteOnUIThread(Action action)
        {
            var dispatcher = CoreApplication.Views[0].Dispatcher;
            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                action();
            });
        }
    }
}
