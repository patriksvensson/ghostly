using System;
using System.Threading.Tasks;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.ViewModels.Dialogs;

namespace Ghostly.Uwp.Infrastructure
{
    public sealed class WhatsNewService
    {
        private readonly IDialogService _dialog;
        private readonly IMarketHelper _market;
        private bool _shown;

        public WhatsNewService(IDialogService dialog, IMarketHelper market)
        {
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _market = market ?? throw new ArgumentNullException(nameof(market));
        }

        public async Task Show()
        {
            if (_market.IsUpdated() && !_shown)
            {
                _shown = true;
                await _dialog.ShowDialog(new WhatsNewViewModel.Request());
            }
        }
    }
}
