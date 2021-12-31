using System;

namespace Ghostly.Core.Diagnostics
{
    public interface IGhostlyLog
    {
        bool IsEnabled(LogLevel level);
        IDisposable Push(string key, object value);

        void Write(LogLevel level, string format, params object[] args);
        void Write(LogLevel level, Exception ex, string format, params object[] args);
    }
}
