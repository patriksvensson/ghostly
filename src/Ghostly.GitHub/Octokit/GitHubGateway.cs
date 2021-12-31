using System;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using OctokitClient = Octokit.GitHubClient;
using OctokitRateLimit = Octokit.MiscellaneousRateLimit;
using RateLimitExceededException = Octokit.RateLimitExceededException;

namespace Ghostly.GitHub.Octokit
{
    public interface IGitHubGateway
    {
        bool IsRateLimited { get; }
        DateTime Reset { get; }

        Task<GitHubResult> Execute(Func<OctokitClient, Task> func);
        Task<GitHubResult<T>> Execute<T>(Func<OctokitClient, Task<T>> func);
    }

    internal sealed class GitHubGateway : IGitHubGateway
    {
        private readonly OctokitClient _client;
        private readonly IGhostlyLog _log;

        public bool IsRateLimited => DateTime.UtcNow < Reset;
        public DateTime Reset { get; private set; }

#if CHAOS
        private readonly Random _randomizer = new Random();
#endif

        // Used by the Autofac container
        public delegate IGitHubGateway Factory(OctokitClient client);

        public GitHubGateway(OctokitClient client, IGhostlyLog log)
        {
            _client = client;
            _log = log;

            Reset = DateTime.MinValue;
        }

        public async Task<GitHubResult> Execute(Func<OctokitClient, Task> func)
        {
            try
            {
                if (AreWeBeingRateLimited())
                {
                    return GitHubResult.RateLimited(Reset);
                }

#if CHAOS
                if (_randomizer.Next(0, 100) == 50)
                {
                    if (_randomizer.Next() % 2 == 0)
                    {
                        Reset = DateTime.UtcNow.AddSeconds(15);
                        return GitHubResult.RateLimited(Reset);
                    }
                    else
                    {
                        throw new InvalidOperationException("CHAOS!");
                    }
                }
#endif

                await func(_client);
                return GitHubResult.Ok(true);
            }
            catch (RateLimitExceededException ex)
            {
                Reset = ex.Reset.ToUniversalTime().UtcDateTime.AddSeconds(15);
                return GitHubResult.RateLimited(Reset);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "An error occured while contacting GitHub.");
                return GitHubResult.Err(ex);
            }
        }

        public async Task<GitHubResult<T>> Execute<T>(Func<OctokitClient, Task<T>> func)
        {
            try
            {
                if (typeof(T) != typeof(OctokitRateLimit))
                {
                    if (AreWeBeingRateLimited())
                    {
                        return GitHubResult.RateLimited<T>(Reset);
                    }
                }

#if CHAOS
                if (_randomizer.Next(0, 100) == 50)
                {
                    if (_randomizer.Next() % 2 == 0)
                    {
                        Reset = DateTime.UtcNow.AddSeconds(15);
                        return GitHubResult.RateLimited<T>(Reset);
                    }
                    else
                    {
                        throw new InvalidOperationException("CHAOS!");
                    }
                }
#endif

                var result = await func(_client);
                return GitHubResult.Ok(result);
            }
            catch (RateLimitExceededException ex)
            {
                Reset = ex.Reset.ToUniversalTime().UtcDateTime.AddSeconds(15);
                return GitHubResult.RateLimited<T>(Reset);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "An error occured while contacting GitHub.");
                return GitHubResult.Err<T>(ex);
            }
        }

        private bool AreWeBeingRateLimited()
        {
            if (IsRateLimited)
            {
                if (DateTime.UtcNow > Reset)
                {
                    Reset = DateTime.MinValue;
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }
}