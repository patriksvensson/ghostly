using System;
using System.Collections.Generic;
using Ghostly.Core.Mvvm;
using Ghostly.Features.Querying;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Ghostly.Uwp.Controls
{
    public sealed partial class QueryControl : UserControl
    {
        private readonly List<AutoCompleteItem> _words;
        private readonly AutoCompleteState _state;

        public IReadOnlyCollection<AutoCompleteItem> Suggestions => _state.Suggestions;
        public IReadOnlyState<int> SuggestionCount => _state.Count;

        public QueryControl()
        {
            InitializeComponent();

            _words = new List<AutoCompleteItem>(GhostlyQueryLanguage.GetKeywords());
            _state = new AutoCompleteState(() =>
            {
                return SuggestionList.SelectedItem as AutoCompleteItem;
            }, _words);

            Window.Current.CoreWindow.KeyDown += Global_KeyDown;
        }

        public void ClearQuery()
        {
            Query.Text = string.Empty;
        }

        private void Global_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (SuggestionPopup.IsOpen)
            {
                if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.Tab)
                {
                    InsertSelectedItem(allowFirst: true);
                }
                else if (Query.FocusState != FocusState.Unfocused && args.VirtualKey == VirtualKey.Down)
                {
                    SuggestionList.Focus(FocusState.Programmatic);
                    SuggestionList.SelectedIndex = 0;
                }
            }
            else
            {
                if (args.VirtualKey == VirtualKey.Enter)
                {
                    if (!string.IsNullOrWhiteSpace(Query.Text))
                    {
                        if (Compile)
                        {
                            if (!GhostlyQueryLanguage.TryCompile(Query.Text, out var _, out var error))
                            {
                                Error = error;
                                return;
                            }
                        }

                        OnQuerySubmitted(this, new QuerySubmittedEventArgs(Query.Text.Trim()));
                    }
                }
            }
        }

        private void Query_PreviewKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (CoreWindow.GetForCurrentThread().IsCtrlKeyPressed())
            {
                if ((int)e.Key == 190)
                {
                    TryShowPopup(acceptEmpty: true);
                }
                else if (e.Key == VirtualKey.Space)
                {
                    e.Handled = true;
                    TryShowPopup(acceptEmpty: true);
                }
            }
            else if (e.Key == VirtualKey.Down)
            {
                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Escape)
            {
                SuggestionPopup.Close(Dispatcher);
                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Tab)
            {
                if (SuggestionPopup.IsOpen)
                {
                    InsertSelectedItem(allowFirst: true);
                    e.Handled = true;
                }
            }
        }

        private void SuggestionList_PreviewKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (SuggestionPopup.IsOpen)
            {
                if (e.Key == VirtualKey.Escape && SuggestionPopup.IsOpen)
                {
                    SuggestionPopup.Close(Dispatcher);
                    Query.Focus(FocusState.Programmatic);
                    e.Handled = true;
                }
                else if (e.Key == VirtualKey.Tab)
                {
                    InsertSelectedItem(allowFirst: false);
                    e.Handled = true;
                }
            }
        }

        private void SuggestionList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (SuggestionPopup.IsOpen)
            {
                InsertSelectedItem(allowFirst: false);
                e.Handled = true;
            }
        }

        private void Query_TextChanged(object sender, TextChangedEventArgs e)
        {
            Error = string.Empty;

            _state.UpdateText(Query.Text, Query.SelectionStart);

            if (string.IsNullOrWhiteSpace(Query.Text))
            {
                OnQueryCleared(this, new QueryClearedEventArgs());
                SuggestionPopup.IsOpen = false;
            }
            else
            {
                TryShowPopup();
            }
        }

        private void Query_LosingFocus(UIElement sender, Windows.UI.Xaml.Input.LosingFocusEventArgs args)
        {
            if (args.FocusState != FocusState.Programmatic && SuggestionPopup.IsOpen)
            {
                SuggestionPopup.Close(Dispatcher);
            }
        }

        private void SuggestionList_LosingFocus(UIElement sender, Windows.UI.Xaml.Input.LosingFocusEventArgs args)
        {
            if (args.FocusState != FocusState.Programmatic
                && args.FocusState != FocusState.Keyboard
                && SuggestionPopup.IsOpen)
            {
                SuggestionPopup.Close(Dispatcher);
            }
        }

        private void InsertSelectedItem(bool allowFirst)
        {
            if (_state.TryInsertSelectedItem(allowFirst, out var text, out var selection))
            {
                Query.Text = text;
                Query.SelectionStart = selection;

                SuggestionPopup.Close(Dispatcher);
                Query.Focus(FocusState.Programmatic);
            }
        }

        private void TryShowPopup(bool acceptEmpty = false)
        {
            if (!_state.TryPopulateSuggestions(acceptEmpty, out var bounds))
            {
                SuggestionPopup.Close(Dispatcher);
                return;
            }

            // Calculate the position of the beginning of the word.
            var left = Query.SelectionStart - 1;
            if (bounds.Valid)
            {
                left = Math.Min(bounds.Start, left);
            }

            // Get the selection rectangle.
            var selection = new Rect(0, 0, 0, 19); // Hack to get an initial value that is OK..
            if (left >= 0)
            {
                selection = Query.GetRectFromCharacterIndex(left, false);
            }

            // Calculate the point where to display the popup.
            var transform = SuggestionPopup.TransformToVisual(Query);
            var point = transform.TransformPoint(new Point(selection.Left + 10, selection.Bottom + 10));

            // Got a header?
            if (Query.Header != null)
            {
                // This is a terrible hack, but we need to
                // adjust the position if we're displaying a header.
                point.Y += 22;
            }

            // Reset the selected index if the
            // popup isn't being shown to the user.
            if (!SuggestionPopup.IsOpen)
            {
                SuggestionList.SelectedIndex = 0;
            }

            // Update the popup with the calculated information.
            Dispatcher.FireAndForgetSafe(() =>
            {
                SuggestionPopup.HorizontalOffset = point.X;
                SuggestionPopup.VerticalOffset = point.Y;
                SuggestionPopup.Open();
            });
        }
    }
}
