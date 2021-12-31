using System;

namespace Ghostly.GitHub
{
    public class GitHubResult
    {
        public Exception Exception { get; }
        public DateTime? RateLimitReset { get; }

        public bool Tracked { get; set; }
        public bool Logged { get; set; }

        public bool IsRateLimited => RateLimitReset != null;
        public bool Faulted => IsRateLimited || Exception != null;

        protected GitHubResult(Exception exception, DateTime? rateLimitReset)
        {
            Exception = exception;
            RateLimitReset = rateLimitReset;
        }

        public GitHubResult<TOther> Convert<TOther>(TOther value = default)
        {
            return new GitHubResult<TOther>(value, Exception, RateLimitReset)
            {
                Tracked = Tracked,
                Logged = Logged,
            };
        }

        public static GitHubResult Ok()
        {
            return new GitHubResult(null, null);
        }

        public static GitHubResult Err(Exception ex)
        {
            return new GitHubResult(ex, null);
        }

        public static GitHubResult RateLimited(DateTime reset)
        {
            return new GitHubResult(null, reset);
        }

        public static GitHubResult<T> Ok<T>(T data = default)
        {
            return new GitHubResult<T>(data, null, null);
        }

        public static GitHubResult<T> Err<T>(Exception ex)
        {
            return new GitHubResult<T>(default, ex, null);
        }

        public static GitHubResult<T> RateLimited<T>(DateTime reset)
        {
            return new GitHubResult<T>(default, null, reset);
        }
    }

    public class GitHubResult<T> : GitHubResult
    {
        private readonly T _data;

        public GitHubResult(T data, Exception exception, DateTime? rateLimitReset)
            : base(exception, rateLimitReset)
        {
            _data = data;
        }

        public T Unwrap()
        {
            if (Faulted)
            {
                throw new InvalidOperationException("Cannot unwrap faulted result.");
            }

            return _data;
        }
    }
}
