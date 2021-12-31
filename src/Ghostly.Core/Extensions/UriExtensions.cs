using System;
using System.Linq;

namespace Ghostly
{
    public static class UriExtensions
    {
        public static bool TryGetLastSegment(this string address, out string last)
        {
            if (Uri.TryCreate(address, UriKind.Absolute, out var uri))
            {
                last = uri.Segments.LastOrDefault();
                return last != null;
            }

            last = null;
            return false;
        }

        public static bool TryGetId(this string address, out int id)
        {
            if (Uri.TryCreate(address, UriKind.Absolute, out var uri))
            {
                if (int.TryParse(uri.Segments.Last(), out int issueId))
                {
                    id = issueId;
                    return true;
                }
            }

            id = 0;
            return false;
        }
    }
}
