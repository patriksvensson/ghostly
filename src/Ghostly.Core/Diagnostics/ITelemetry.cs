using System;
using System.Collections.Generic;

namespace Ghostly.Core.Diagnostics
{
    public interface ITelemetry
    {
        void EnableTelemetry(bool enable);

        void TrackPageView(string pageName);
        void TrackDiagnostic(LogLevel level, string message, IDictionary<string, string> properties = null);
        void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);
        void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);
    }
}
