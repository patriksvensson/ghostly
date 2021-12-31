using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;

namespace Ghostly.Startup
{
    public sealed class StartupLogger : IStartup
    {
        private readonly IGhostlyLog _log;
        private readonly IPackageService _package;
        private readonly IMarketHelper _market;
        private readonly ISystemService _system;
        private readonly IThemeHelper _theme;
        private readonly INetworkHelper _network;
        private readonly ITelemetry _telemetry;

        public StartupLogger(
            IGhostlyLog log,
            IPackageService package,
            IMarketHelper market,
            ISystemService system,
            IThemeHelper theme,
            INetworkHelper network,
            ITelemetry telemetry)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _market = market ?? throw new ArgumentNullException(nameof(market));
            _system = system ?? throw new ArgumentNullException(nameof(system));
            _theme = theme ?? throw new ArgumentNullException(nameof(theme));
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _telemetry = telemetry;
        }

        public Task Start(bool background)
        {
            if (background)
            {
                return Task.CompletedTask;
            }

            var version = _package.GetVersion();
            var firstRun = _market.IsFirstRun();
            var justUpdated = _market.IsUpdated();
            var theme = _theme.GetTheme().ToString();
            var hasNetworkConnection = _network.IsConnected;
            var metered = _network.IsMetered;

            var uptime = _system.Uptime;
            if (uptime.ToInvariantString().StartsWith("-", StringComparison.OrdinalIgnoreCase))
            {
                uptime = TimeSpan.Zero;
            }

            _telemetry.TrackEvent(Constants.TrackingEvents.AppStarted,
                new Dictionary<string, string>
                {
                    { "Version", version },
                    { "FirstRun", _system.IsFirstRun.ToYesNo() },
                    { "JustUpdated", _system.IsUpdated.ToYesNo() },
                    { "Theme", theme },
                    { "Connected", hasNetworkConnection.ToYesNo() },
                    { "Metered", metered.ToYesNo() },
                    { "Manufacturer", _system.Manufacturer },
                    { "Model", _system.Model },
                    { "LaunchCount", _system.LaunchCount.ToInvariantString() },
                    { "Culture", _system.Culture },
                    { "FirstVersion", _package.GetFirstInstalledVersion() },
                    { "FirstUsed", _system.FirstUsed.ToInvariantString("yyyy-MM-dd HH:mm:ss") },
                    { "LastLaunchTime", _system.LastLaunchTime.ToInvariantString("yyyy-MM-dd HH:mm:ss") },
                    { "LastResetTime", _system.LastResetTime.ToInvariantString("yyyy-MM-dd HH:mm:ss") },
                    { "LaunchTime", _system.LaunchTime.ToInvariantString("yyyy-MM-dd HH:mm:ss") },
                    { "TotalLaunchCount", _system.TotalLaunchCount.ToInvariantString() },
                    { "Uptime", uptime.ToInvariantString(@"dd\:hh\:mm\:ss") },
                });

            _log.Information("Version={GhostlyVersion}, First run={IsFirstRun}, Just updated={JustUpdated}, " +
                "Theme={GhostlyTheme}, Has network connection={HasNetworkConnection}, " +
                "Is metered connection={IsMeteredConnection}",
                version, firstRun, justUpdated, theme, hasNetworkConnection,
                _network.IsMetered);

            return Task.CompletedTask;
        }
    }
}
