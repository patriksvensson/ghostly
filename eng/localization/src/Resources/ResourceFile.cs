namespace Ghostly.Tools.Resources
{
    public sealed class ResourceFile
    {
        public string Culture { get; set; }
        public List<ResourceEntry> Entries { get; }

        public ResourceFile(string culture)
        {
            Culture = culture;
            Entries = new List<ResourceEntry>();
        }
    }
}
