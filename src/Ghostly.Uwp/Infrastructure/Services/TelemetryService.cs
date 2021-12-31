using System;
using System.Collections.Generic;
using Ghostly.Core.Diagnostics;

namespace Ghostly.Uwp.Infrastructure
{
    public sealed class TelemetryService : ITelemetry
    {
        public static TelemetryService Instance { get; } = new TelemetryService();

        public void EnableTelemetry(bool enable)
        {
        }

        public void TrackDiagnostic(LogLevel level, string message, IDictionary<string, string> properties = null)
        {
        }

        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }

        public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }

        public void TrackPageView(string pageName)
        {
        }

        internal void Flush()
        {
        }
    }
}
