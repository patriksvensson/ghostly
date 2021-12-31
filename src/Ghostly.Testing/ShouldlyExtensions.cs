using System;

namespace Ghostly.Testing
{
    public static class ShouldlyExtensions
    {
        public static T And<T>(this T source)
        {
            return source;
        }

        public static void And<T>(this T source, Action<T> action)
        {
            if (source != null && action != null)
            {
                action(source);
            }
        }
    }
}
