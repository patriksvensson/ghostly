#addin "nuget:?package=Scriban&version=5.0.0"
using Scriban;

Task("Generate-Secrets")
    .Does(ctx =>
{
    var clientId = Argument<string>("client-id");   
    var clientSecret = Argument<string>("client-secret");

    var template = Template.Parse(System.IO.File.ReadAllText("./eng/templates/secrets.scriban"));

    // Render template
    var result = template.Render(new { ClientId = clientId, ClientSecret = clientSecret });
    if (result == null)
    {
        throw new InvalidOperationException("An error occured while rendering template");
    }

    // Write result
    var output = MakeAbsolute(File("./src/Ghostly.Github/GitHubSecrets.Generated.cs"));
    System.IO.File.WriteAllText(output.FullPath, result);
});

RunTarget("Generate-Secrets");