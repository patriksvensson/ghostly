using System;

namespace Ghostly.Core.Diagnostics
{
    public static class LogExtensions
    {
        public static void Fatal(this IGhostlyLog log, string format, params object[] args)
        {
            log?.Write(LogLevel.Fatal, format, args);
        }

        public static void Error(this IGhostlyLog log, string format, params object[] args)
        {
            log?.Write(LogLevel.Error, format, args);
        }

        public static void Error(this IGhostlyLog log, Exception ex)
        {
            log?.Write(LogLevel.Error, ex, null);
        }

        public static void Error(this IGhostlyLog log, Exception ex, string format, params object[] args)
        {
            log?.Write(LogLevel.Error, ex, format, args);
        }

        public static void Warning(this IGhostlyLog log, string format, params object[] args)
        {
            log?.Write(LogLevel.Warning, format, args);
        }

        public static void Information(this IGhostlyLog log, string format, params object[] args)
        {
            log?.Write(LogLevel.Information, format, args);
        }

        public static void Verbose(this IGhostlyLog log, string format, params object[] args)
        {
            log?.Write(LogLevel.Verbose, format, args);
        }

        public static void Debug(this IGhostlyLog log, string format, params object[] args)
        {
            log?.Write(LogLevel.Debug, format, args);
        }
    }
}
