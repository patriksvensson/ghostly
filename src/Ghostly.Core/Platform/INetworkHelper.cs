namespace Ghostly.Core.Pal
{
    public interface INetworkHelper
    {
        bool IsConnected { get; }
        bool IsMetered { get; }

        string GetHostName();
    }
}
