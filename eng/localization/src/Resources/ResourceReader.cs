using System.Collections;
using System.Resources;

namespace Ghostly.Tools.Resources
{
    public static class ResourceReader
    {
        public static HashSet<ResourceEntry> Read(Stream stream)
        {
            var result = new HashSet<ResourceEntry>();

            using (var reader = new ResXResourceReader(stream))
            {
                var resolver = new StringTypeResolutionService();
                reader.UseResXDataNodes = true;

                var enumerator = reader.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var current = (DictionaryEntry)enumerator.Current;
                    var node = current.Value as ResXDataNode;
                    if (node == null)
                    {
                        continue;
                    }

                    result.Add(new ResourceEntry(
                        node.Name,
                        node.GetValue(resolver)?.ToString() ?? string.Empty,
                        node.Comment));
                }
            }

            return result;
        }
    }
}
