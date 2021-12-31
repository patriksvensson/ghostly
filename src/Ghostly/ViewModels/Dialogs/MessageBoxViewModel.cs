using Ghostly.Core.Mvvm;

namespace Ghostly.ViewModels.Dialogs
{
    public enum MessageBoxResult
    {
        Ok,
    }

    public sealed class MessageBoxViewModel : DialogScreen<MessageBoxViewModel.Request, MessageBoxResult>
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Glyph { get; set; }
        public string PrimaryText { get; set; }

        public sealed class Request : IDialogRequest<MessageBoxResult>
        {
            public string Title { get; set; }
            public string Body { get; set; }
            public string Glyph { get; set; }
            public string PrimaryText { get; set; }
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
        }

        public override MessageBoxResult GetResult(bool ok)
        {
            return MessageBoxResult.Ok;
        }
    }
}
