using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;
using Ghostly.Tools.Resources;
using Ghostly.Tools.Utilities;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.IO;

namespace Ghostly.Tools.Commands.Import
{
    public sealed class ImportCommand : Command<ImportCommand.Settings>
    {
        private readonly IFileSystem _fileSystem;
        private readonly IEnvironment _environment;

        public sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "<INPUT>")]
            [TypeConverter(typeof(DirectoryPathConverter))]
            public DirectoryPath? Input { get; set; }

            [CommandArgument(1, "<OUTPUT>")]
            [TypeConverter(typeof(DirectoryPathConverter))]
            public DirectoryPath? Output { get; set; }
        }

        public ImportCommand(IFileSystem fileSystem, IEnvironment environment)
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

            settings.Input = settings!.Input?.MakeAbsolute(_environment);
            settings.Output = settings!.Output?.MakeAbsolute(_environment);

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

            AnsiConsole.Status()
                .Start("Importing...", ctx =>
                {


                    var files = _fileSystem.Directory.GetFiles(settings.Input!, "*.json", SearchScope.Current).ToArray();
                    foreach (var file in files)
                    {
                        var filename = file.Path.GetFilename().FullPath;

                        ctx.Status($"Processing [yellow]{filename}[/]...");
                        AnsiConsole.MarkupLine($"Processing [yellow]{filename}[/]");

                        if (!IsValidCulture(file.Path))
                        {
                            AnsiConsole.MarkupLine(
                                "[yellow]WARNING:[/] The file [yellow]{0}[/] does not represent a valid culture",
                                file.Path.GetFilename().FullPath);
                            continue;
                        }

                        // Read the JSON content.
                        var json = ReadJson(file.Path);
                        var model = JsonConvert.DeserializeObject<ResourceFile>(json);
                        if (model == null)
                        {
                            throw new InvalidOperationException($"Could not read JSON file '{filename}'");
                        }

                        // Does the output directory exist?
                        var output = settings!.Output!.Combine(new DirectoryPath(file.Path.GetFilenameWithoutExtension().FullPath.ToLowerInvariant()));
                        if (!_fileSystem.Directory.Exists(output))
                        {
                            AnsiConsole.MarkupLine($"Creating output directory [yellow]{output.GetDirectoryName()}[/]");
                            _fileSystem.Directory.Create(output);
                        }

                        // Write the resource file.
                        var outputFilePath = output.CombineWithFilePath(new FilePath("Resources.resw"));
                        AnsiConsole.MarkupLine($"Writing resource file to disk");
                        WriteResource(model, outputFilePath);
                    }
                });


            return 0;
        }

        private void WriteResource(ResourceFile model, FilePath outputFilePath)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (outputFilePath is null)
            {
                throw new ArgumentNullException(nameof(outputFilePath));
            }

            var writer = new ResXResourceWriter(outputFilePath.FullPath);
            foreach (var entry in model.Entries.OrderBy(x => x.Key))
            {
                writer.AddResource(
                    new ResXDataNode(entry.Key, entry.Translation)
                    {
                        Comment = entry.Comment,
                    });
            }

            writer.Close();
        }

        private bool IsValidCulture(FilePath path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            try
            {
#pragma warning disable CA1806 // Do not ignore method results
                new CultureInfo(path.GetFilenameWithoutExtension().FullPath);
#pragma warning restore CA1806 // Do not ignore method results
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string ReadJson(FilePath path)
        {
            using (var stream = _fileSystem.File.OpenRead(path))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
