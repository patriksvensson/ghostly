using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Ghostly
{
    public static class StringExtensions
    {
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Used after scope")]
        public static async Task<IInputStream> ToInputStreamAsync(this string input)
        {
            var stream = new InMemoryRandomAccessStream();
            using (var writer = new DataWriter(stream))
            {
                writer.WriteString(input);
                await writer.StoreAsync();
                writer.DetachStream();
            }

            return stream.GetInputStreamAt(0);
        }
    }
}
