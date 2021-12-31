using System;
using Microsoft.Extensions.Logging;
using GhostlyLogLevel = Ghostly.Core.Diagnostics.LogLevel;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Ghostly.Diagnostics
{
    internal sealed class DatabaseLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly DatabaseLogProvider _provider;

        public DatabaseLogger(string categoryName, DatabaseLogProvider provider)
        {
            _provider = provider;
            _categoryName = categoryName;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!_provider.Configuration.Logger.IsEnabled(GhostlyLogLevel.Verbose))
            {
                return;
            }

            // Grab a reference to the current logger settings for consistency,
            // and to eliminate the need to block a thread reconfiguring the logger
            var config = _provider.Configuration;
            if (config.Filter(_categoryName, logLevel))
            {
                config.Logger.Write(
                    GhostlyLogLevel.Verbose,
                    "{DatabaseMessage}",
                    formatter(state, exception));
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}