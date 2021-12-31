using System;
using Ghostly.Core.Diagnostics;
using Serilog.Events;

namespace Ghostly.Diagnostics
{
    public static class LogLevelExtensions
    {
        public static LogEventLevel GetSerilogLogEventLevel(this LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Fatal:
                    return LogEventLevel.Fatal;
                case LogLevel.Error:
                    return LogEventLevel.Error;
                case LogLevel.Warning:
                    return LogEventLevel.Warning;
                case LogLevel.Information:
                    return LogEventLevel.Information;
                case LogLevel.Verbose:
                    return LogEventLevel.Verbose;
                case LogLevel.Debug:
                    return LogEventLevel.Debug;
                default:
                    throw new InvalidOperationException("Unknown log level.");
            }
        }
    }
}
