using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Ghostly.Core;
using Ghostly.Core.Mvvm;

namespace Ghostly.Features.Querying
{
    public sealed class AutoCompleteState
    {
        private readonly IReadOnlyList<AutoCompleteItem> _items;
        private readonly ObservableCollection<AutoCompleteItem> _suggestions;
        private readonly Stateful<int> _count;
        private readonly Func<AutoCompleteItem> _selectedItem;

        public string Text { get; private set; }
        public int Position { get; private set; }

        public IReadOnlyCollection<AutoCompleteItem> Suggestions => _suggestions;
        public IReadOnlyState<int> Count => _count;

        public AutoCompleteState(Func<AutoCompleteItem> func, IEnumerable<AutoCompleteItem> words)
        {
            _items = words.ToReadOnlyList();
            _suggestions = new ObservableCollection<AutoCompleteItem>();
            _count = new Stateful<int>(0);
            _selectedItem = func ?? throw new ArgumentNullException(nameof(func));

            UpdateText(string.Empty, 0);
        }

        public void UpdateText(string text, int position)
        {
            Text = text ?? string.Empty;
            Position = position;
        }

        public bool TryPopulateSuggestions(bool allowShowingAll, out WordBoundaries boundaries)
        {
            boundaries = default;

            try
            {
                var position = Math.Min(Text.Length - 1, Position - 1);
                var current = Text.GetPartialWordAtPosition(position);

                // Current token is empty and the previous
                // one is a ':'? Show the popup to display options.
                if (string.IsNullOrWhiteSpace(current) && Text.TryGetTokenAtPosition(position, out var token))
                {
                    if (token == ':')
                    {
                        allowShowingAll = true;
                    }
                    else
                    {
                        current = token.ToString(CultureInfo.InvariantCulture);
                    }
                }

                var haveText = !string.IsNullOrEmpty(Text) && !string.IsNullOrWhiteSpace(current);
                if (!allowShowingAll && !haveText)
                {
                    _suggestions.Clear();
                    return false;
                }

                var hits = _items.Where(x => x.Name.StartsWith(current, StringComparison.OrdinalIgnoreCase));

                boundaries = Text.GetWordBoundaries(position);
                if (boundaries.Valid)
                {
                    // Previous token ':'?
                    if (Text.TryGetTokenAtPosition(boundaries.Start - 1, out token))
                    {
                        if (token == ':')
                        {
                            // Previous word 'is'?
                            var previousWord = Text.GetPartialWordAtPosition(boundaries.Start - 2);
                            if (previousWord?.Equals("is", StringComparison.OrdinalIgnoreCase) ?? false)
                            {
                                // TODO: Filter out anything not boolean
                                hits = hits.Where(x => x.ReturnType != null && x.ReturnType == typeof(bool));
                            }
                            else
                            {
                                // We've entered a contains expression.
                                // Let's bail.
                                _suggestions.Clear();
                                return false;
                            }
                        }
                    }
                }

                var count = Suggestions.Count;

                _suggestions.Clear();
                _suggestions.AddRange(hits.ToList());

                // Update count?
                if (_suggestions.Count != count)
                {
                    _count.Value = _suggestions.Count;
                }

                // Just a single item that is an exact match?
                // Don't show the popup in that case.
                if (Suggestions.Count == 1)
                {
                    if (_suggestions[0].Name.Equals(current, StringComparison.OrdinalIgnoreCase))
                    {
                        _suggestions.Clear();
                    }
                }

                return _suggestions.Count > 0;
            }
            catch
            {
                _suggestions.Clear();
                return false;
            }
        }

        public bool TryInsertSelectedItem(bool allowFirst, out string text, out int selection)
        {
            text = null;
            selection = -1;

            try
            {
                var selectedItem = _selectedItem();
                if (selectedItem == null)
                {
                    if (allowFirst)
                    {
                        selectedItem = _suggestions.FirstOrDefault();
                    }
                }

                if (selectedItem == null)
                {
                    return false;
                }

                var left = Math.Max(0, Math.Min(Text.Length - 1, Position));
                var right = Math.Max(0, Math.Min(Text.Length - 1, Position));
                var haveExactMatch = false;

                if (!char.IsLetterOrDigit(selectedItem.Name[0]))
                {
                    // Got an exact match?
                    if (Position - selectedItem.Name.Length >= 0)
                    {
                        var exact = Text.Substring(Position - selectedItem.Name.Length, selectedItem.Name.Length);
                        if (exact == selectedItem.Name)
                        {
                            left = Position - selectedItem.Name.Length;
                            right = Position;
                            haveExactMatch = true;
                        }
                    }

                    if (!haveExactMatch)
                    {
                        // Got a partial match?
                        if (Text[Position - 1] == selectedItem.Name[0])
                        {
                            left = Position - 1;
                            right = Position;
                            haveExactMatch = true;
                        }
                    }
                }

                if (!haveExactMatch)
                {
                    var position = Math.Max(0, Math.Min(Text.Length - 1, Position - 1));
                    var boundaries = Text.GetWordBoundaries(position);
                    if (!boundaries.Valid)
                    {
                        return false;
                    }

                    left = boundaries.Start;
                    right = boundaries.End;

                    if (left > 0 && char.IsLetterOrDigit(Text[left - 1]))
                    {
                        left += 1;
                        right = Math.Max(left, right);
                    }
                }

                text = Text.Remove(left, right - left);
                text = text.Insert(left, selectedItem.Name);

                selection = Math.Min(left + selectedItem.Name.Length, text.Length);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
