using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace Ghostly.Services.Templating
{
    public class GitHubUsernameParser : InlineParser
    {
        public GitHubUsernameParser()
        {
            OpeningCharacters = new[] { '@' };
        }

        public class Extension : IMarkdownExtension
        {
            public void Setup(MarkdownPipelineBuilder pipeline)
            {
                if (pipeline is null)
                {
                    throw new System.ArgumentNullException(nameof(pipeline));
                }

                if (!pipeline.InlineParsers.Contains<GitHubUsernameParser>())
                {
                    pipeline.InlineParsers.Add(new GitHubUsernameParser());
                }
            }

            public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
            {
            }
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            if (processor is null)
            {
                throw new System.ArgumentNullException(nameof(processor));
            }

            // Previous character can't be a letter or number
            var previous = slice.PeekCharExtra(-1);
            if (previous.IsAlphaNumeric())
            {
                return false;
            }

            // Traverse
            var current = slice.NextChar();
            var start = slice.Start;
            var end = slice.Start;
            while (!current.IsWhiteSpaceOrZero() && (current.IsAlphaNumeric() || current == '-' || current == '_'))
            {
                end = slice.Start;
                current = slice.NextChar();
            }

            // Just an @ sign?
            if (start == end)
            {
                return false;
            }

            // Get the username and create the link.
            var username = new StringSlice(slice.Text, start, end).ToString();

            var link = new LinkInline()
            {
                Span =
                {
                    Start = processor.GetSourcePosition(slice.Start, out var line, out var column),
                },
                Line = line,
                Column = column,
                IsClosed = true,
            };
            link.Span.End = link.Span.Start + username.Length;

            link.Url = $"https://github.com/{username}";
            link.AppendChild(new LiteralInline($"@{username}"));

            // Return the link
            processor.Inline = link;
            return true;
        }
    }
}
