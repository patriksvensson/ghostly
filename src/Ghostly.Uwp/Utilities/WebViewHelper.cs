using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Ghostly.Uwp.Utilities
{
    public sealed class WebViewHelper
    {
        // "HtmlString" attached property for a WebView
        public static readonly DependencyProperty HtmlStringProperty =
           DependencyProperty.RegisterAttached("HtmlString", typeof(string), typeof(WebViewHelper), new PropertyMetadata(string.Empty, OnHtmlStringChanged));

        // Getter and Setter
        public static string GetHtmlString(DependencyObject obj)
        {
            if (obj != null)
            {
                return (string)obj.GetValue(HtmlStringProperty);
            }

            return null;
        }

        public static void SetHtmlString(DependencyObject obj, string value)
        {
            if (obj != null)
            {
                obj.SetValue(HtmlStringProperty, value);
            }
        }

        private static void OnHtmlStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WebView webView)
            {
                var uri = webView.BuildLocalStreamUri("Ghostly", "html");
                StreamUriResolver.Instance.Html = (string)e.NewValue;
                webView.NavigateToLocalStreamUri(uri, StreamUriResolver.Instance);
            }
        }
    }
}
