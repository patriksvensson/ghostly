#addin "nuget:?package=Cake.MinVer&version=2.0.0"
#load "eng/cake/utilities.cake"

using Spectre.Console;

var config = Argument("configuration", "Release");
var fullBuild = HasArgument("full");
var patchAppX = HasArgument("patch") ? true : fullBuild;
var platforms = fullBuild ? "x86|x64|arm|arm64" : "x86|x64|arm";
var artifacts = MakeAbsolute(new DirectoryPath("./.artifacts"));
var packages = artifacts.Combine("packages");

Teardown(ctx => {
    Information("Shutting down .NET core SDK tooling...");
    DotNetBuildServerShutdown(
        new DotNetBuildServerShutdownSettings {
            MSBuild = true
        });
});

Task("Clean")
    .Does(ctx => 
{
    CleanDirectory(artifacts);
    CleanDirectory(packages);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(ctx => 
{
    MSBuild("./src/Ghostly.sln", new MSBuildSettings()
        .WithTarget("Restore")
        .SetConfiguration(config)
        .SetVerbosity(Verbosity.Quiet)
        .SetNoLogo(true)
        .WithProperty("RestoreAdditionalProjectFallbackFolders", "")
        .WithProperty("AppxBundle","Always")
        .WithProperty("UapAppxPackageBuildMode", "StoreUpload")
        .WithProperty("AppxBundlePlatforms", platforms)
        .UseVSWhere(ctx));
});

Task("Patch-AppX-Info")
    .WithCriteria(() => patchAppX, "Not running full build")
    .IsDependentOn("Restore")
    .Does(ctx => 
{
    var version = MinVer();
    var identity = fullBuild ? $"Ghostly" : "Ghostly_Dev";
    var name = fullBuild ? $"Ghostly" : "Ghostly (Dev)";

    var update = new Dictionary<string, string> {
        { "c:Package/c:Identity/@Name", fullBuild ? $"SpectreSystemsAB.Ghostly" : "SpectreSystemsAB.Ghostly.Dev"},
        { "c:Package/c:Applications/c:Application[1]/uap:VisualElements/@DisplayName", name },
        { "c:Package/c:Applications/c:Application[1]/uap:VisualElements/uap:DefaultTile/@ShortName", name },
        { "c:Package/c:Identity/@Version", $"{version.Major}.{version.Minor}.{version.Patch}.0" },
        { "c:Package/c:Identity/@Publisher", "CN=BD765503-99E8-40AA-91EC-B3E295890BB4" },
        { "c:Package/c:Applications/c:Application[1]/c:Extensions/uap5:Extension/uap5:StartupTask/@TaskId", identity },
        { "c:Package/c:Applications/c:Application[1]/c:Extensions/uap5:Extension/uap5:StartupTask/@DisplayName", name }
    };

    var manifest = File("./src/Ghostly.Uwp/Package.appxmanifest");
    foreach(var item in update) {
        XmlPoke(manifest, item.Key, item.Value,
        new XmlPokeSettings {
            Namespaces = {
                { "c", "http://schemas.microsoft.com/appx/manifest/foundation/windows10" },
                { "uap", "http://schemas.microsoft.com/appx/manifest/uap/windows10" },
                { "uap5", "http://schemas.microsoft.com/appx/manifest/uap/windows10/5" },
            }
        });
    }
});

Task("Test")
    .IsDependentOn("Restore")
    .Does(ctx =>
{
    var projects = GetFiles("./src/*.Tests/*.Tests.csproj");
    foreach(var project in projects) {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"Running tests: [yellow]{project.GetFilename().FullPath}[/]");
        DotNetTest(project.FullPath, new DotNetTestSettings {
            Configuration = config,
            NoRestore = true,
            NoLogo = true,
            Verbosity = DotNetVerbosity.Quiet,
        });
    }
});

Task("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Patch-AppX-Info")
    .Does(ctx => 
{
    MSBuild("./src/Ghostly.sln", new MSBuildSettings()
        .SetConfiguration(config)
        .SetVerbosity(Verbosity.Quiet)
        .SetNoLogo(true)
        .UseToolVersion(MSBuildToolVersion.VS2022)
        .SetMSBuildPlatform(MSBuildPlatform.x64)
        .EnableBinaryLogger()
        .WithProperty("RestoreAdditionalProjectFallbackFolders", "")
        .WithProperty("AppxBundle","Always")
        .WithProperty("UapAppxPackageBuildMode", "StoreUpload")
        .WithProperty("AppxBundlePlatforms", platforms)
        .SetUwpOutput(packages)
        .UseNativeToolchain(fullBuild)
        .BuildAppXBundle()
        .UseVSWhere(ctx));
});

RunTarget(Argument("target", "Build"));