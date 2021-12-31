using System;
using Ghostly.Core.Diagnostics;

namespace Ghostly.Diagnostics
{
    public class NullLog : IGhostlyLog
    {
        private sealed class DummyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public bool IsEnabled(LogLevel level)
        {
            return true;
        }

        public IDisposable Push(string key, object value)
        {
            return new DummyDisposable();
        }

        public void Write(LogLevel level, string format, params object[] args)
        {
        }

        public void Write(LogLevel level, Exception ex, string format, params object[] args)
        {
        }
    }
}
