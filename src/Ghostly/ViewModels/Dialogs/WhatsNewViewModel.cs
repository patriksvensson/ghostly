using Ghostly.Core.Mvvm;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;

namespace Ghostly.ViewModels.Dialogs
{
    public sealed class WhatsNewViewModel : DialogScreen<WhatsNewViewModel.Request, bool>
    {
        public string Message { get; set; }

        public sealed class Request : IDialogRequest<bool>
        {
        }

        public WhatsNewViewModel(IPackageService package, ILocalizer localizer)
        {
            if (package is null)
            {
                throw new System.ArgumentNullException(nameof(package));
            }

            if (localizer is null)
            {
                throw new System.ArgumentNullException(nameof(localizer));
            }

            Message = localizer.Format("WhatsNew_GhostlyWasUpdated", package.GetVersion());
        }

        public override void Initialize(Request request)
        {
        }

        public override bool GetResult(bool ok)
        {
            return true;
        }
    }
}
