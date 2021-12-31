namespace Ghostly.Core.Pal
{
    public interface ILocalSettings
    {
        bool LogSql { get; }

        T GetValue<T>(string key, T defaultValue = default);
        void SetValue<T>(string key, T value);
    }
}
