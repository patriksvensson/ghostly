using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using IGhostlyLog = Ghostly.Core.Diagnostics.IGhostlyLog;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Ghostly.Diagnostics
{
    internal sealed class DatabaseLogProvider : ILoggerProvider
    {
        public LoggingConfiguration Configuration { get; }

        private DatabaseLogProvider(IGhostlyLog logger, Func<string, LogLevel, bool> filter)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            Configuration = new LoggingConfiguration(logger, filter);
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Logger factory take ownership")]
        public static void Create(ILoggerFactory factory, IGhostlyLog logger, Func<string, LogLevel, bool> filter)
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            factory.AddProvider(new DatabaseLogProvider(logger, filter));
        }

        public class LoggingConfiguration
        {
            public IGhostlyLog Logger { get; }
            public Func<string, LogLevel, bool> Filter { get; }

            public LoggingConfiguration(IGhostlyLog logger, Func<string, LogLevel, bool> filter)
            {
                Logger = logger;
                Filter = filter;
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DatabaseLogger(categoryName, this);
        }

        public void Dispose()
        {
        }
    }
}