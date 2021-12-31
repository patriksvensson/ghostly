using Ghostly.Core.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Ghostly.Diagnostics
{
    public sealed class LogLevelSwitch : LoggingLevelSwitch, ILogLevelSwitch
    {
        public LogLevelSwitch(LogEventLevel initialMinimumLevel = LogEventLevel.Information)
            : base(initialMinimumLevel)
        {
        }

        public void SetMinimumLevel(LogLevel level)
        {
            MinimumLevel = level.GetSerilogLogEventLevel();
        }
    }
}
