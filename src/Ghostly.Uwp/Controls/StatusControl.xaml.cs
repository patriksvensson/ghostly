using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Ghostly.Uwp.Controls
{
    public sealed partial class StatusControl : UserControl
    {
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string),
                typeof(StatusControl),
                new PropertyMetadata(string.Empty, OnMessageChanged));

        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register(nameof(Percentage), typeof(int),
                typeof(StatusControl), new PropertyMetadata(0, OnPercentageChanged));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public int Percentage
        {
            get { return (int)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        public StatusControl()
        {
            InitializeComponent();

            Visibility = Visibility.Collapsed;
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string stringValue && string.IsNullOrWhiteSpace(stringValue))
            {
                ((StatusControl)d).Visibility = Visibility.Collapsed;
            }
            else
            {
                ((StatusControl)d).Visibility = Visibility.Visible;
            }
        }

        private static void OnPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is int intValue)
            {
                if (((StatusControl)d).FindName("Progress") is ProgressBar progress)
                {
                    if (intValue < 0)
                    {
                        progress.IsIndeterminate = true;

                        if (((StatusControl)d).FindName("ProgressText") is TextBlock text)
                        {
                            if (text.Visibility != Visibility.Collapsed)
                            {
                                text.Text = string.Empty;
                                text.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                    else
                    {
                        progress.IsIndeterminate = false;
                        progress.Value = intValue;
                        progress.Maximum = 100;

                        if (((StatusControl)d).FindName("ProgressText") is TextBlock text)
                        {
                            if (text.Visibility != Visibility.Visible)
                            {
                                text.Visibility = Visibility.Visible;
                            }

                            text.Text = $"{intValue}%";
                        }
                    }
                }
            }
        }
    }
}
