using System;
using System.Collections.Generic;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using GhostlyLogLevel = Ghostly.Core.Diagnostics.LogLevel;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Ghostly.Data
{
    public interface IGhostlyContextFactory
    {
        GhostlyContext Create();
    }

    public sealed class GhostlyContextFactory : IGhostlyContextFactory
    {
        private readonly GhostlyContext.Factory _factory;
        private readonly ILocalSettings _settings;
        private readonly IGhostlyLog _log;

        public GhostlyContextFactory(
            GhostlyContext.Factory factory,
            ILocalSettings settings,
            IGhostlyLog log)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public GhostlyContext Create()
        {
            var context = _factory();
            if (_settings.LogSql && _log.IsEnabled(GhostlyLogLevel.Verbose))
            {
                ConfigureLogging(context, _log);
            }

            return context;
        }

        private static readonly List<string> _categories = new List<string>
        {
            DbLoggerCategory.Query.Name,
            DbLoggerCategory.Database.Command.Name,
            DbLoggerCategory.Update.Name,
        };

        private static void ConfigureLogging(GhostlyContext context, IGhostlyLog log)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var serviceProvider = context.GetInfrastructure();
            var loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));

            DatabaseLogProvider.Create(loggerFactory, log, (category, level) =>
            {
                if (level != MicrosoftLogLevel.Information)
                {
                    return false;
                }

                return _categories.Contains(category);
            });
        }
    }
}
