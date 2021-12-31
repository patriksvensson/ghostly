using System;
using Autofac;
using Ghostly.Core.Diagnostics;
using Serilog;
using Serilog.Formatting.Compact;

namespace Ghostly.Diagnostics
{
    public sealed class LogModule : Module
    {
        private readonly LogLevel _level;
        private readonly string _path;

        public LogModule(LogLevel level, string path)
        {
            _level = level;
            _path = path;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // Create the switch.
            var @switch = new LogLevelSwitch(_level.GetSerilogLogEventLevel());
            builder.RegisterInstance(@switch).As<ILogLevelSwitch>().SingleInstance();

            var config = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .MinimumLevel.ControlledBy(@switch)
#if DEBUG
                .WriteTo.Seq("http://localhost:5341")
#endif
                .Enrich.WithProperty("Session", Guid.NewGuid().ToString());

            if (!string.IsNullOrWhiteSpace(_path))
            {
                config.WriteTo.File(
                    formatter: new CompactJsonFormatter(),
                    path: $"{_path}\\Logs\\Ghostly-.log",
                    rollingInterval: RollingInterval.Day);
            }

            // Create the logger
            Log.Logger = config.CreateLogger();

            builder.RegisterType<SerilogAdapter>().As<IGhostlyLog>().SingleInstance();
            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
        }
    }
}
