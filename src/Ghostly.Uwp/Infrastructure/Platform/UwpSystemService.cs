using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Pal;
using Microsoft.Toolkit.Uwp.Helpers;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    [SuppressMessage("Design", "CA1812", Justification = "Instantiated via DI")]
    internal sealed class UwpSystemService : ISystemService, IInitializable
    {
        public bool IsFirstRun { get; private set; }
        public bool IsUpdated { get; private set; }
        public string Culture { get; private set; }
        public string Model { get; private set; }
        public string Manufacturer { get; private set; }
        public long LaunchCount { get; private set; }
        public long TotalLaunchCount { get; private set; }
        public TimeSpan Uptime { get; private set; }
        public DateTime FirstUsed { get; private set; }
        public DateTime LaunchTime { get; private set; }
        public DateTime LastLaunchTime { get; private set; }
        public DateTime LastResetTime { get; private set; }

        public Task<bool> Initialize(bool background)
        {
            IsFirstRun = GetSafe(() => SystemInformation.Instance.IsFirstRun);
            IsUpdated = GetSafe(() => SystemInformation.Instance.IsAppUpdated);
            Culture = GetSafe(() => SystemInformation.Instance.Culture?.Name, "Unknown culture");
            Model = GetSafe(() => SystemInformation.Instance.DeviceModel, "Unknown device model");
            Manufacturer = GetSafe(() => SystemInformation.Instance.DeviceManufacturer, "Unknown manufacturer");
            LaunchCount = GetSafe(() => SystemInformation.Instance.LaunchCount);
            TotalLaunchCount = GetSafe(() => SystemInformation.Instance.TotalLaunchCount);
            Uptime = GetSafe(() => SystemInformation.Instance.AppUptime, TimeSpan.Zero);
            FirstUsed = GetSafe(() => SystemInformation.Instance.FirstUseTime, DateTime.MinValue);
            LaunchTime = GetSafe(() => SystemInformation.Instance.LaunchTime, DateTime.MinValue);
            LastLaunchTime = GetSafe(() => SystemInformation.Instance.LastLaunchTime, DateTime.MinValue);
            LastResetTime = GetSafe(() => SystemInformation.Instance.LastResetTime, DateTime.MinValue);

            return Task.FromResult(true);
        }

        private T GetSafe<T>(Func<T> value, T defaultValue = default)
        {
            try
            {
                return value();
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
