namespace Ghostly.Core.Services
{
    public interface ILocalizer
    {
        string this[string key] { get; }
        string GetString(string key);
    }
}
