using Ghostly.Tools.Commands;
using Ghostly.Tools.Commands.Import;
using Ghostly.Tools.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.IO;

namespace Ghostly.Tools
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandApp(CreateRegistrar());
            app.Configure(config =>
            {
                config.SetExceptionHandler(ex =>
                {
                    AnsiConsole.WriteException(ex);
                });

                config.AddCommand<CheckCommand>("check");
                config.AddCommand<ExportCommand>("export");
                config.AddCommand<ImportCommand>("import");
            });

            return app.Run(args);
        }

        private static ITypeRegistrar CreateRegistrar()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton<IEnvironment, Spectre.IO.Environment>();
            services.AddSingleton<IPlatform, Platform>();
            services.AddSingleton<IGlobber, Globber>();
            services.AddSingleton<DirectoryPathConverter>();
            services.AddSingleton<FilePathConverter>();

            return new TypeRegistrar(services);
        }
    }
}
