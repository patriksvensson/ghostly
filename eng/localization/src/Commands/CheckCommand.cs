using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Ghostly.Tools.Resources;
using Ghostly.Tools.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.IO;
using ResourceReader = Ghostly.Tools.Resources.ResourceReader;

namespace Ghostly.Tools.Commands
{
    public sealed class CheckCommand : Command<CheckCommand.Settings>
    {
        private readonly IFileSystem _fileSystem;
        private readonly IEnvironment _environment;

        public sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "<PATH>")]
            [TypeConverter(typeof(DirectoryPathConverter))]
            public DirectoryPath? Input { get; set; }
        }

        public CheckCommand(IFileSystem fileSystem, IEnvironment environment)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public override ValidationResult Validate([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            settings.Input = settings.Input?.MakeAbsolute(_environment);

            if (settings.Input == null)
            {
                return ValidationResult.Error("Input directory has not been provided.");
            }

            return ValidationResult.Success();
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            bool valid = true;
            var result = new Dictionary<CultureInfo, ResourceFile>();

            return AnsiConsole.Status()
                .Start("Checking...", ctx =>
                {
                    // TODO: Validate
                    foreach (var folder in _fileSystem.GetDirectory(settings.Input!).GetDirectories("*", SearchScope.Current))
                    {
                        var culture = GetCultureFromFolderName(folder);
                        if (culture == null)
                        {
                            continue;
                        }

                        ctx.Status($"Parsing [yellow]{culture.Name}[/]...");

                        // TODO: Validate
                        var resourceFilePath = folder.Path.CombineWithFilePath(new FilePath("Resources.resw"));
                        using (var stream = _fileSystem.File.OpenRead(resourceFilePath))
                        {
                            var entries = ResourceReader.Read(stream);
                            var file = new ResourceFile(culture.Name);
                            foreach (var entry in entries.OrderBy(x => x.Key))
                            {
                                file.Entries.Add(entry);
                            }

                            // Add the parsed info to the result list
                            result.Add(culture, file);
                        }
                    }

                    // Get the template file
                    var templateResource = result.Where(x => x.Key.Name == "en-US").Select(x => x.Value).FirstOrDefault();
                    if (templateResource == null)
                    {
                        AnsiConsole.MarkupLine($"[red]ERROR:[/] Could not find template language [yellow]en-US[/]");
                        valid = false;
                    }
                    else
                    {
                        // Check for missing entries
                        foreach (var item in result.Values.Where(x => x.Culture != "en-US"))
                        {
                            ctx.Status($"Checking [yellow]{item.Culture}[/] for missing entries...");
                            AnsiConsole.MarkupLine($"Checking [yellow]{item.Culture}[/] for missing entries...");

                            foreach (var templateEntry in templateResource.Entries)
                            {
                                // Any discrepancies between comment and translation in the template?
                                if (templateEntry.Translation != templateEntry.Comment)
                                {
                                    var tree = new Tree("[red]ERROR:[/] Found discrepancy between text and comment in [yellow]en-US[/]");
                                    var key = tree.AddNode($"[yellow]{templateEntry.Key}[/]");
                                    key.AddNode($"   TEXT: [red]{templateEntry.Translation.EscapeMarkup()}[/]");
                                    key.AddNode($"COMMENT: [red]{templateEntry.Comment.EscapeMarkup()}[/]");
                                    AnsiConsole.Write(tree);
                                    valid = false;
                                }

                                var destinationEntry = item.Entries.SingleOrDefault(x => x.Key == templateEntry.Key);
                                if (destinationEntry == null)
                                {
                                    AnsiConsole.MarkupLine($"[yellow]WARNING:[/] The key [yellow]{templateEntry.Key}[/] is missing in [yellow]{item.Culture}[/]");
                                    valid = false;
                                }
                            }
                        }
                    }

                    if (!valid)
                    {
                        return -1;
                    }

                    AnsiConsole.MarkupLine("[green]Everything is OK![/]");
                    return 0;
                });
        }

        private static CultureInfo? GetCultureFromFolderName(IDirectory folder)
        {
            try
            {
                return new CultureInfo(folder.Path.GetDirectoryName());
            }
            catch (CultureNotFoundException)
            {
                return null;
            }
        }
    }
}
