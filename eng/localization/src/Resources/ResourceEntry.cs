namespace Ghostly.Tools.Resources
{
    public sealed class ResourceEntry
    {
        public string Key { get; }
        public string Translation { get; }
        public string Comment { get; set; }

        public ResourceEntry(string key, string translation, string comment)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Translation = translation ?? string.Empty;
            Comment = comment ?? string.Empty;
        }
    }
}
