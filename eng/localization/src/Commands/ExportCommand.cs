using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Ghostly.Tools.Resources;
using Ghostly.Tools.Utilities;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.IO;
using ResourceReader = Ghostly.Tools.Resources.ResourceReader;

namespace Ghostly.Tools.Commands
{
    public sealed class ExportCommand : Command<ExportCommand.Settings>
    {
        private readonly IFileSystem _fileSystem;
        private readonly IEnvironment _environment;

        public sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "<PATH>")]
            [TypeConverter(typeof(DirectoryPathConverter))]
            public DirectoryPath? Input { get; set; }

            [CommandArgument(1, "<PATH>")]
            [TypeConverter(typeof(DirectoryPathConverter))]
            public DirectoryPath? Output { get; set; }
        }

        public ExportCommand(IFileSystem fileSystem, IEnvironment environment)
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
            settings.Output = settings.Output?.MakeAbsolute(_environment);

            if (settings.Input == null)
            {
                return ValidationResult.Error("Input directory has not been provided.");
            }
            else if (!_fileSystem.Directory.Exists(settings.Input))
            {
                return ValidationResult.Error("Input directory do not exist.");
            }

            if (settings.Output == null)
            {
                return ValidationResult.Error("Output directory has not been provided.");
            }
            else if (!_fileSystem.Directory.Exists(settings.Output))
            {
                return ValidationResult.Error("Output directory do not exist.");
            }

            return ValidationResult.Success();
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var result = new Dictionary<CultureInfo, ResourceFile>();

            return AnsiConsole.Status()
                .Start("Exporting...", ctx =>
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

                            // Add the parsed info to the result list.
                            result.Add(culture, file);
                        }
                    }

                    // Get the template file.
                    var templateResource = result.Where(x => x.Key.Name == "en-US").Select(x => x.Value).FirstOrDefault();
                    if (templateResource == null)
                    {
                        throw new InvalidOperationException("Could not find template language 'en-US'.");
                    }

                    // Add missing entries.
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
                                return 1;
                            }

                            var destinationEntry = item.Entries.SingleOrDefault(x => x.Key == templateEntry.Key);
                            if (destinationEntry == null)
                            {
                                // For english languages, just copy the translation.
                                var translation = item.Culture.StartsWith("en", StringComparison.OrdinalIgnoreCase)
                                    ? templateEntry.Comment
                                    : string.Empty;

                                AnsiConsole.MarkupLine($"Adding [yellow]{templateEntry.Key}[/] to [yellow]{item.Culture}[/]");
                                item.Entries.Add(new ResourceEntry(templateEntry.Key, translation, templateEntry.Comment));
                            }
                        }
                    }

                    // Update comments for all templates.
                    foreach (var item in result)
                    {
                        foreach (var entry in item.Value.Entries)
                        {
                            var templateEntry = templateResource.Entries.SingleOrDefault(x => x.Key == entry.Key);
                            if (templateEntry != null && entry.Comment != templateEntry.Comment)
                            {
                                entry.Comment = templateEntry.Comment;
                            }
                        }
                    }

                    // Write all files to disk.
                    foreach (var item in result)
                    {
                        var filename = new FilePath(item.Key.Name).AppendExtension("json");
                        WriteJson(settings!.Output!.Combine(new DirectoryPath("translations")).CombineWithFilePath(filename), item.Value);
                    }

                    // Create a template file.
                    var template = new ResourceFile("replace-me");
                    foreach (var entry in templateResource.Entries)
                    {
                        template.Entries.Add(new ResourceEntry(entry.Key, string.Empty, entry.Comment));
                    }

                    WriteJson(settings!.Output!.CombineWithFilePath(new FilePath("template.json")), template);

                    return 0;
                });
        }

        private void WriteJson(FilePath path, ResourceFile file)
        {
            var entries = file.Entries.OrderBy(x => x.Key).ToList();
            file.Entries.Clear();
            file.Entries.AddRange(entries);

            var json = JsonConvert.SerializeObject(file, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            });

            using (var outputStream = _fileSystem.File.OpenWrite(path))
            using (var writer = new StreamWriter(outputStream))
            {
                writer.Write(json);
            }
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
