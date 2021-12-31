using System;

namespace Ghostly.Core.Pal
{
    public interface ISystemService
    {
        bool IsFirstRun { get; }
        bool IsUpdated { get; }

        string Culture { get; }

        string Model { get; }
        string Manufacturer { get; }

        long LaunchCount { get; }
        long TotalLaunchCount { get; }
        TimeSpan Uptime { get; }

        DateTime FirstUsed { get; }
        DateTime LaunchTime { get; }
        DateTime LastLaunchTime { get; }
        DateTime LastResetTime { get; }
    }
}
