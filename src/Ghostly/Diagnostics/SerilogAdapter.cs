using System;
using Ghostly.Core.Diagnostics;
using Serilog;
using Serilog.Context;

namespace Ghostly.Diagnostics
{
    public sealed class SerilogAdapter : IGhostlyLog
    {
        private readonly ILogger _logger;

        public SerilogAdapter(ILogger logger)
        {
            _logger = logger;
        }

        public bool IsEnabled(LogLevel level)
        {
            return _logger.IsEnabled(level.GetSerilogLogEventLevel());
        }

        public IDisposable Push(string key, object value)
        {
            return LogContext.PushProperty(key, value);
        }

        public void Write(LogLevel level, string format, params object[] args)
        {
            switch (level)
            {
                case LogLevel.Fatal:
                    _logger.Fatal(format, args);
                    return;
                case LogLevel.Error:
                    _logger.Error(format, args);
                    return;
                case LogLevel.Warning:
                    _logger.Warning(format, args);
                    return;
                case LogLevel.Information:
                    _logger.Information(format, args);
                    return;
                case LogLevel.Verbose:
                    _logger.Verbose(format, args);
                    return;
                case LogLevel.Debug:
                    _logger.Debug(format, args);
                    return;
            }
        }

        public void Write(LogLevel level, Exception ex, string format, params object[] args)
        {
            _logger.Error(ex, format ?? "An error occured.", args);
        }
    }
}