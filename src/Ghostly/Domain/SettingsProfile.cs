using System.Collections.Generic;
using System.Text;
using Ghostly.Features.Categories;
using Markdig.Helpers;
using Newtonsoft.Json;

namespace Ghostly.Domain
{
    public sealed class SettingsProfile
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("categories")]
        public List<SettingsProfileCategory> Categories { get; }

        [JsonProperty("rules")]
        public List<SettingsProfileRule> Rules { get; }

        public SettingsProfile()
        {
            Categories = new List<SettingsProfileCategory>();
            Rules = new List<SettingsProfileRule>();
        }

        public string GetFilename()
        {
            var builder = new StringBuilder();
            foreach (var character in Name)
            {
                if (character.IsAlphaNumeric() || character == '-')
                {
                    builder.Append(character);
                }
                else if (character.IsWhitespace())
                {
                    builder.Append('_');
                }
            }

            builder.Append(".json");
            return builder.ToString();
        }
    }

    public sealed class SettingsProfileCategory
    {
        [JsonProperty("id")]
        public string Identifier { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("emoji")]
        public string Emoji { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }

        [JsonProperty("sort_order")]
        public int SortOrder { get; set; }

        [JsonProperty("show_total")]
        public bool ShowTotal { get; set; }

        [JsonProperty("muted")]
        public bool Muted { get; set; }

        [JsonProperty("include_muted")]
        public bool IncludeMuted { get; set; }

        public CreateCategoryHandler.Request GetCreateRequest(string profile)
        {
            return new CreateCategoryHandler.Request(Name, Filter, Emoji, ShowTotal, Muted, IncludeMuted)
            {
                Identifier = Identifier,
                SortOrder = SortOrder,
                ImportedFrom = profile,
            };
        }

        public UpdateCategoryHandler.Request GetUpdateRequest(string profile, int categoryId)
        {
            return new UpdateCategoryHandler.Request(categoryId, Name, Filter, Emoji, ShowTotal, Muted, IncludeMuted)
            {
                SortOrder = SortOrder,
                ImportedFrom = profile,
            };
        }
    }

    public sealed class SettingsProfileRule
    {
        [JsonProperty("id")]
        public string Identifier { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("sort_order")]
        public int SortOrder { get; set; }

        [JsonProperty("expression")]
        public string Expression { get; set; }

        [JsonProperty("star")]
        public bool Star { get; set; }

        [JsonProperty("mute")]
        public bool Mute { get; set; }

        [JsonProperty("mark_as_read")]
        public bool MarkAsRead { get; set; }

        [JsonProperty("stop_processing")]
        public bool StopProcessing { get; set; }

        [JsonProperty("category_id")]
        public string CategoryIdentifier { get; set; }
    }
}
