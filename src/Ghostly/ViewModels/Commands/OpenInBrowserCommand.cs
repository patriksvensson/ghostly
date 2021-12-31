using System;
using System.Threading.Tasks;
using Ghostly.Core.Mvvm.Commands;
using Ghostly.Core.Pal;
using Ghostly.Domain;

namespace Ghostly.ViewModels.Commands
{
    public sealed class OpenInBrowserCommand : AsyncCommand<Notification>
    {
        private readonly IUriLauncher _launcher;

        public OpenInBrowserCommand(IUriLauncher launcher)
        {
            _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        }

        protected override async Task ExecuteCommand(Notification parameter)
        {
            if (parameter?.Url != null)
            {
                await _launcher.Launch(parameter.Url);
            }
        }
    }
}
