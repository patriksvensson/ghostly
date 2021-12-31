using System;
using System.Globalization;

namespace Ghostly.Core
{
    public static class GuidExtensions
    {
        /// <summary>
        /// Formats the string as Ghostly want GUIDS to be formatted.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns>A string representing the GUID.</returns>
        public static string ToGhostlyFormat(this Guid guid)
        {
            return guid.ToString("N", CultureInfo.InvariantCulture);
        }
    }
}
