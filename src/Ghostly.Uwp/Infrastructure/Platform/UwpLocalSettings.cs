using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Diagnostics;
using Ghostly.Domain.Messages;
using Newtonsoft.Json;
using Serilog;
using Windows.Storage;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    public sealed class UwpLocalSettings : ILocalSettings
    {
        private readonly IMessageService _messenger;
        private readonly IGhostlyLog _log;

        public bool LogSql { get; }

        public UwpLocalSettings(
            IMessageService messenger,
            IGhostlyLog log,
            IFileSystem fileSystem)
        {
            if (fileSystem is null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            LogSql = fileSystem.FileExist("log-sql");
        }

        private sealed class DummyMessageService : IMessageService
        {
            public Task Publish<T>(T message, Func<Func<Task>, Task> marshal)
            {
                throw new NotSupportedException();
            }

            public void Subscribe<T>(Func<T, Task> handler, Func<Func<Task>, Task> marshal)
            {
                throw new NotSupportedException();
            }
        }

        // TODO: Hack. Fix this.
        internal static T GetValue<T>(Func<UwpLocalSettings, T> func, T defaultValue = default)
        {
            try
            {
                var settings = new UwpLocalSettings(new DummyMessageService(), new SerilogAdapter(Log.Logger), new UwpFileSystem());
                return func(settings);
            }
            catch
            {
                return defaultValue;
            }
        }

        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var value))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>((string)value);
                }
                catch
                {
                    _log.Warning("Could not deserialize value {SettingsValue} for key {SettingsKey}", value ?? "null", key);
                }
            }

            return defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = JsonConvert.SerializeObject(value);
            _messenger.Publish(new SettingUpdated(key));
        }
    }
}
