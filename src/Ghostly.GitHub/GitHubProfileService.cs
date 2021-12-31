using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;
using Ghostly.GitHub.Octokit;
using Newtonsoft.Json;
using GistFileUpdate = Octokit.GistFileUpdate;
using GistUpdate = Octokit.GistUpdate;
using NewGist = Octokit.NewGist;
using OctokitGist = Octokit.Gist;

namespace Ghostly.GitHub
{
    internal sealed class GitHubProfileService : IVendorProfiles
    {
        private readonly IGitHubGatewayFactory _github;
        private readonly IPasswordVault _passwordVault;
        private readonly INetworkHelper _network;
        private readonly IGhostlyLog _log;

        public GitHubProfileService(
            IGitHubGatewayFactory factory,
            IPasswordVault passwordVault,
            INetworkHelper network,
            IGhostlyLog log)
        {
            _github = factory ?? throw new ArgumentNullException(nameof(factory));
            _passwordVault = passwordVault ?? throw new ArgumentNullException(nameof(passwordVault));
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<IReadOnlyList<SettingsProfile>> GetProfiles(Account account)
        {
            _log.Information("Getting Ghostly profiles from GitHub...");

            var gateway = await GetGateway(account);
            if (gateway == null)
            {
                _log.Error("Could not create GitHub gateway.");
                return Array.Empty<SettingsProfile>();
            }

            var gist = await GetGist(gateway, includeContent: true);
            if (gist == null)
            {
                _log.Debug("No GitHub Gist could be found containing profiles.");
                return Array.Empty<SettingsProfile>();
            }

            return GetProfilesFromGist(gist);
        }

        public async Task<bool> Export(Account account, SettingsProfile profile)
        {
            _log.Information("Exporting profile {ProfileName} to GitHub...", profile.Name);

            var gateway = await GetGateway(account);
            if (gateway == null)
            {
                _log.Error("Could not create GitHub gateway.");
                return false;
            }

            // Get all existing profiles except the one we're adding.
            var profiles = new List<SettingsProfile>();
            var gist = await GetGist(gateway, includeContent: false);
            if (gist == null)
            {
                // Create new gist
                _log.Debug("Creating GitHub Gist containing profile...");
                var gistResult = await gateway.Execute(client => client.Gist.Create(GetNewGist(profile)));
                if (gistResult.Faulted)
                {
                    _log.Error("Could not create GitHub Gist. Does the user have the correct permissions?");
                    return false;
                }

                gist = gistResult.Unwrap();
            }
            else
            {
                _log.Debug("Updating GitHub Gist {GistId} with new profile information...", gist.Id);
                var gistResult = await gateway.Execute(client => client.Gist.Edit(gist.Id, new GistUpdate
                {
                    Files =
                    {
                        {
                            profile.GetFilename(), new GistFileUpdate
                            {
                                Content = JsonConvert.SerializeObject(profile, Formatting.Indented),
                            }
                        },
                    },
                }));

                if (gistResult.Faulted)
                {
                    _log.Error("Could not update GitHub Gist {GistId}. Does the user have the correct permissions?", gist.Id);
                    return false;
                }
            }

            _log.Information("Profile {ProfileName} was exported to GitHub Gist {GistId}.", profile.Name, gist.Id);
            return true;
        }

        private async Task<IGitHubGateway> GetGateway(Account model)
        {
            if (!(model is GitHubAccount account))
            {
                throw new InvalidOperationException("Unknown account type");
            }

            // Get the token
            var token = _passwordVault.GetGitHubAccessToken(account.Username);
            if (token == null)
            {
                _log.Error("Could not get GitHub access token from password vault.");
                return null;
            }

            // No internet connection?
            if (!_network.IsConnected)
            {
                _log.Error("No network connection available.");
                return null;
            }

            // Rate limited?
            var gateway = _github.Create(token);
            if (gateway.IsRateLimited)
            {
                _log.Error("We've been rate limited by GitHub.");
                return null;
            }

            // Not authorized?
            if (!await gateway.IsAuthorized())
            {
                _log.Error("GitHub account has not been authorized.");
                return null;
            }

            return gateway;
        }

        private async Task<OctokitGist> GetGist(IGitHubGateway gateway, bool includeContent)
        {
            _log.Debug("Getting GitHub Gist containing profiles...");

            var gistsResult = await gateway.Execute(x => x.Gist.GetAll(new DateTimeOffset(2020, 06, 25, 0, 0, 0, TimeSpan.Zero)));
            if (gistsResult.Faulted)
            {
                _log.Error("Could not get GitHub Gists. Does the user have the correct permissions?");
                return null;
            }

            foreach (var gist in gistsResult.Unwrap())
            {
                if (gist.Description == "Ghostly profiles (generated)")
                {
                    _log.Debug("Found Ghostly profiles in Gist {GistId}", gist.Id);

                    if (!includeContent)
                    {
                        return gist;
                    }

                    _log.Debug("Getting GitHub Gist content...");
                    var gistResult = await gateway.Execute(x => x.Gist.Get(gist.Id));
                    if (gistResult.Faulted)
                    {
                        _log.Error("Could not get content of GitHub Gist.");
                        return null;
                    }

                    return gistResult.Unwrap();
                }
            }

            // No profiles were found.
            _log.Debug("Ghostly profile Gist was not found");
            return null;
        }

        private IReadOnlyList<SettingsProfile> GetProfilesFromGist(OctokitGist gist)
        {
            _log.Debug("Extracting profiles from GitHub Gist {GistId}...", gist.Id);

            var result = new List<SettingsProfile>();

            foreach (var file in gist.Files)
            {
                if (file.Value.Language?.Equals("json", StringComparison.OrdinalIgnoreCase) != true)
                {
                    continue;
                }

                _log.Debug("Extracting profile from {GistFilename}", file.Key);

                var json = file.Value.Content;
                if (string.IsNullOrWhiteSpace(json))
                {
                    _log.Verbose("Profile was empty.");
                    continue;
                }

                try
                {
                    result.Add(JsonConvert.DeserializeObject<SettingsProfile>(json));
                }
                catch (Exception exception)
                {
                    _log.Error(exception, "Could not deserialize profile from {GistFilename}. {ProfileJson}", file.Key, json);
                }
            }

            return result;
        }

        private NewGist GetNewGist(SettingsProfile profile)
        {
            var gist = new NewGist
            {
                Description = "Ghostly profiles (generated)",
                Public = false,
            };

            gist.Files.Add(".ghostly", "This GitHub Gist was generated by Ghostly.\nEdit at your own risk.");
            gist.Files.Add(profile.GetFilename(), JsonConvert.SerializeObject(profile, Formatting.Indented));

            return gist;
        }
    }
}
