using Ghostly.Core.Mvvm;

namespace Ghostly.ViewModels.Dialogs
{
    public enum ConfirmResult
    {
        Cancel = 0,
        Ok,
    }

    public sealed class ConfirmActionViewModel : DialogScreen<ConfirmActionViewModel.Request, ConfirmResult>
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Glyph { get; set; }

        public string PrimaryText { get; set; }
        public string SecondaryText { get; set; }

        public sealed class Request : IDialogRequest<ConfirmResult>
        {
            public string Title { get; set; }
            public string Body { get; set; }
            public string Glyph { get; set; }

            public string PrimaryText { get; set; }
            public string SecondaryText { get; set; }
        }

        public override void Initialize(Request request)
        {
            if (request is null)
            {
                throw new System.ArgumentNullException(nameof(request));
            }

            Title = request.Title;
            Body = request.Body;
            Glyph = request.Glyph;
            PrimaryText = request.PrimaryText;
            SecondaryText = request.SecondaryText;
        }

        public override ConfirmResult GetResult(bool ok)
        {
            return ok ? ConfirmResult.Ok : ConfirmResult.Cancel;
        }
    }
}
