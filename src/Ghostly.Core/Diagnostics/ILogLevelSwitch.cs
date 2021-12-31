namespace Ghostly.Core.Diagnostics
{
    public interface ILogLevelSwitch
    {
        void SetMinimumLevel(LogLevel level);
    }
}
