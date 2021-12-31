using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ghostly.Core;
using Markdig;
using Scriban.Runtime;

namespace Ghostly.Services.Templating.Contexts
{
    public class MarkdownImports : ScriptObject
    {
        private static readonly MarkdownPipeline _pipeline;

        static MarkdownImports()
        {
            _pipeline = new MarkdownPipelineBuilder()
                .UsePipeTables()
                .UseEmojiAndSmiley()
                .UseAutoLinks()
                .UseTaskLists()
                .UseEmphasisExtras()
                .Use<GitHubUsernameParser.Extension>()
                .Build();
        }

        [ScriptMemberIgnore]
        public static string ConvertToHtml(string markdown, Func<string> fallback)
        {
            if (fallback is null)
            {
                throw new ArgumentNullException(nameof(fallback));
            }

            if (string.IsNullOrWhiteSpace(markdown))
            {
                return string.Concat("<i>", fallback(), "</i>");
            }

            return Markdown.ToHtml(markdown, _pipeline);
        }

        public static IEnumerable SortByTimestamp(IEnumerable<IHaveTimestamp> source)
        {
            return source.OrderBy(comment => comment.Timestamp);
        }

        public static int GetCount(ICollection comments)
        {
            return comments?.Count ?? 0;
        }

        public static string FullDate(DateTime timestamp)
        {
            return timestamp.ToLocalTime().ToString("G", CultureInfo.CurrentCulture);
        }
    }
}
