using Ghostly.Core.Mvvm;

namespace Ghostly.ViewModels
{
    public sealed class ProgressViewModel
    {
        public Stateful<string> Message { get; }
        public Stateful<int> Percentage { get; }
        public Stateful<bool> Show { get; }

        public ProgressViewModel(bool show = true)
        {
            Message = new Stateful<string>(string.Empty);
            Percentage = new Stateful<int>(0);
            Show = new Stateful<bool>(show);
        }

        public void Update(string message, int percentage)
        {
            Message.Value = message;
            Percentage.Value = percentage;
            Show.Value = !string.IsNullOrWhiteSpace(message);
        }
    }
}
