using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ghostly.Core.Diagnostics
{
    public static class TelemetryExtensions
    {
        public static void TrackException(
            this ITelemetry telemetry, Exception exception, string caller,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = null,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null,
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (telemetry != null)
            {
                var properties = new Dictionary<string, string>
                {
                    { "Handled", "yes" },
                    { "Caller", caller + "." + (memberName ?? "Unknown") },
                    { "File", sourceFilePath ?? "Unknown" },
                    { "LineNumber", sourceLineNumber.ToString(CultureInfo.InvariantCulture) },
                };

                telemetry?.TrackException(exception, properties);
            }
        }

        public static void TrackAndLogError(
            this ITelemetry telemetry, IGhostlyLog log, string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                telemetry?.TrackDiagnostic(LogLevel.Error, message);
                log?.Error(message);
            }
        }
    }
}
