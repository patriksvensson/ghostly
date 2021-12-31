namespace Ghostly.Domain.Messages
{
    public sealed class NetworkConnectivityChanged
    {
        public bool IsConnected { get; }

        public NetworkConnectivityChanged(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}
