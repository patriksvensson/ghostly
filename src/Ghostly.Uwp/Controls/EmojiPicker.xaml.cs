using System.Collections.Generic;
using System.Linq;
using Ghostly.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Ghostly.Uwp.Controls
{
    public sealed partial class EmojiPicker : UserControl
    {
        public IReadOnlyList<string> Emojis { get; }

        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register(nameof(Selected), typeof(string),
                typeof(EmojiPicker), null);

        public string Selected
        {
            get { return (string)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        public static readonly DependencyProperty DefaultGlyphProperty =
            DependencyProperty.Register(nameof(DefaultGlyph), typeof(string),
                typeof(EmojiPicker), null);

        public string DefaultGlyph
        {
            get { return (string)GetValue(DefaultGlyphProperty); }
            set { SetValue(DefaultGlyphProperty, value); }
        }

        public EmojiPicker()
        {
            InitializeComponent();

            Emojis = Emoji.Supported.Where(x => !x.Deprecated).Select(x => x.Value).ToList();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Selected = null;
            EmojiPopup.IsOpen = false;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            EmojiPopup.IsOpen = !EmojiPopup.IsOpen;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            EmojiPopup.IsOpen = false;
        }

        private void EmojiGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EmojiPopup.IsOpen = false;
        }
    }
}
