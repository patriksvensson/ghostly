using System;
using System.Collections.Generic;
using System.Globalization;
using Ghostly.Core.Diagnostics;

namespace Ghostly.GitHub
{
    internal sealed class GitHubResultTracker
    {
        private readonly GitHubResult _result;
        private readonly string _caller;

        public GitHubResultTracker(GitHubResult result, string caller)
        {
            _result = result ?? throw new ArgumentNullException(nameof(result));
            _caller = caller ?? throw new ArgumentNullException(nameof(caller));

            if (string.IsNullOrWhiteSpace(_caller))
            {
                _caller = "Unknown";
            }

            _caller = caller.Trim();
        }

        public GitHubResult GetResult()
        {
            return _result;
        }

        public GitHubResultTracker Track(ITelemetry telemetry, string message,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = null,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null,
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (telemetry != null && _result.Faulted && !_result.Tracked)
            {
                _result.Tracked = true;

                // Rate limited?
                if (_result.IsRateLimited)
                {
                    // We don't really care about rate limited calls.
                    // TODO: Raise event?
                }

                // Exception?
                else if (_result.Exception != null)
                {
                    var properties = new Dictionary<string, string>
                    {
                        { "Caller", string.Concat(_caller, ".", memberName ?? "Unknown") },
                        { "File", sourceFilePath ?? "Unknown" },
                        { "LineNumber", sourceLineNumber.ToString(CultureInfo.InvariantCulture) },
                    };

                    if (!string.IsNullOrEmpty(message))
                    {
                        properties.Add("Message", message ?? "No message");
                    }

                    telemetry.TrackException(_result.Exception, properties);
                }
            }

            return this;
        }

        public GitHubResultTracker Log(IGhostlyLog log, string format, params object[] args)
        {
            if (log != null && _result.Faulted)
            {
#if !DEBUG
                if (_result.Logged)
                {
                    return this;
                }
#endif

                if (_result.IsRateLimited)
                {
                    log.Error($"Rate limited. {format}", args);
                }
                else if (_result.Exception != null)
                {
                    log.Error(_result.Exception, format, args);
                }
            }

            return this;
        }
    }

    internal sealed class GitHubResultTracker<T>
    {
        private readonly GitHubResultTracker _tracker;

        public GitHubResultTracker(GitHubResult result, string caller)
        {
            _tracker = new GitHubResultTracker(result, caller);
        }

        public GitHubResult<T> GetResult()
        {
            return _tracker.GetResult() as GitHubResult<T>;
        }

        public GitHubResultTracker Track(ITelemetry telemetry, string message,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = null,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null,
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            return _tracker.Track(telemetry, message, memberName, sourceFilePath, sourceLineNumber);
        }

        public GitHubResultTracker Log(IGhostlyLog log, string format, params object[] args)
        {
            return _tracker.Log(log, format, args);
        }
    }
}
