using Windows.UI.Xaml;

namespace Ghostly.Uwp.Controls
{
    public sealed partial class QueryControl
    {
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register(nameof(PlaceholderText), typeof(string),
                typeof(QueryControl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string),
                typeof(QueryControl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string),
            typeof(QueryControl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register(nameof(Error), typeof(string),
            typeof(QueryControl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty CompileProperty =
            DependencyProperty.Register(nameof(Compile), typeof(bool),
            typeof(QueryControl), new PropertyMetadata(true));

        public string PlaceholderText
        {
            get { return (string)GetValue(PlaceholderTextProperty); }
            set { SetValue(PlaceholderTextProperty, value); }
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        public bool Compile
        {
            get { return (bool)GetValue(CompileProperty); }
            set { SetValue(CompileProperty, value); }
        }
    }
}
