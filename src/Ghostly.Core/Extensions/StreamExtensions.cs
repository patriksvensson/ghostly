using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Ghostly
{
    public static class StringExtensions
    {
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Returning stream")]
        public static Stream ToStream(this string text)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
