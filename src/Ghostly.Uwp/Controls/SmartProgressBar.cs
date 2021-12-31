using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Ghostly.Uwp.Controls
{
    public sealed class SmartProgressBar : ProgressBar
    {
        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register(nameof(Percentage), typeof(int),
                typeof(SmartProgressBar), new PropertyMetadata(0, OnPercentageChanged));

        public int Percentage
        {
            get { return (int)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        private static void OnPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is int intValue)
            {
                if (d is ProgressBar progress)
                {
                    if (intValue < 0)
                    {
                        progress.IsIndeterminate = true;
                    }
                    else
                    {
                        progress.IsIndeterminate = false;
                        progress.Value = intValue;
                        progress.Maximum = 100;
                    }
                }
            }
        }
    }
}
