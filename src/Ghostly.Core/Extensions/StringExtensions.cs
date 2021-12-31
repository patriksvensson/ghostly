using System;
using System.Globalization;

namespace Ghostly.Core
{
    public static class StringExtensions
    {
        public static string OrIfNullOrWhiteSpace(this string text, string other)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            return other;
        }

        public static string ToBase64(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            byte[] buffer = System.Text.Encoding.Unicode.GetBytes(text);
            return Convert.ToBase64String(buffer);
        }

        public static string FromBase64(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            byte[] buffer = Convert.FromBase64String(text);
            return System.Text.Encoding.Unicode.GetString(buffer);
        }

        public static string ToInvariantString<T>(this T obj)
            where T : IFormattable
        {
            return obj?.ToString(null, CultureInfo.InvariantCulture);
        }

        public static string ToInvariantString<T>(this T obj, string format)
            where T : IFormattable
        {
            return obj?.ToString(format, CultureInfo.InvariantCulture);
        }

        public static WordBoundaries GetWordBoundaries(this string text, int position)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (position == 0 && text.Length == 0)
            {
                return new WordBoundaries(0, 0);
            }

            if (position >= text.Length)
            {
                return default;
            }

            if (position < 0)
            {
                return default;
            }

            if (text[position] == ' ')
            {
                return new WordBoundaries(position, position);
            }

            if (char.IsSymbol(text[position]))
            {
                if (position > 0 && !char.IsLetterOrDigit(text[position - 1]))
                {
                    return new WordBoundaries(position - 1, position);
                }

                return default;
            }

            var current = position;
            var start = 0;
            while (current >= 0)
            {
                if (char.IsWhiteSpace(text[current]) ||
                    !char.IsLetterOrDigit(text[current]))
                {
                    start = current + 1;
                    break;
                }

                current--;
            }

            current = position;
            var end = text.Length;
            while (current < text.Length)
            {
                if (text[current] == ' ')
                {
                    end = current;
                    break;
                }

                current++;
            }

            if (start == 0 && end == 0)
            {
                return new WordBoundaries(0, 0);
            }

            if (end < start)
            {
                return default;
            }

            end = Math.Min(end, text.Length);

            return new WordBoundaries(start, end);
        }

        public static bool TryGetTokenAtPosition(this string text, int position, out char token)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            token = '\0';
            if (position >= text.Length)
            {
                return false;
            }

            if (position < 0)
            {
                return false;
            }

            token = text[position];
            return true;
        }

        public static string GetPartialWordAtPosition(this string text, int position)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var bounds = GetWordBoundaries(text, position);
            if (!bounds.Valid)
            {
                return string.Empty;
            }

            var start = Math.Min(bounds.Start, text.Length - 1);
            if (text[start] == ' ')
            {
                return string.Empty;
            }

            var end = Math.Min(position + 1, text.Length);
            return text.Substring(bounds.Start, end - bounds.Start);
        }
    }
}
