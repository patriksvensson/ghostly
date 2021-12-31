using System;

namespace Ghostly.Domain.Messages
{
    public sealed class SettingUpdated
    {
        public string SettingName { get; }

        public SettingUpdated(string name)
        {
            SettingName = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
