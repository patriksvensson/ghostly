using System;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Domain.Messages;
using Windows.Networking;
using Windows.Networking.Connectivity;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    [DependentOn(typeof(DatabaseInitializer))]
    public sealed class UwpNetworkHelper : INetworkHelper, IInitializable, IDisposable
    {
        private readonly IMessageService _messages;

        public bool IsConnected { get; private set; }
        public bool IsMetered { get; private set; }

        public UwpNetworkHelper(IMessageService messages)
        {
            _messages = messages;
            NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;
        }

        public void Dispose()
        {
            NetworkInformation.NetworkStatusChanged -= OnNetworkStatusChanged;
        }

        public Task<bool> Initialize(bool background)
        {
            UpdateNetworkConnectionStatus();
            return Task.FromResult(true);
        }

        private void OnNetworkStatusChanged(object sender)
        {
            UpdateNetworkConnectionStatus();
            _messages.PublishAsync(new NetworkConnectivityChanged(IsConnected));
        }

        private void UpdateNetworkConnectionStatus()
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();
            if (profile != null)
            {
                IsConnected = profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
                IsMetered = profile.GetConnectionCost().NetworkCostType != NetworkCostType.Unrestricted;
            }
            else
            {
                IsConnected = false;
                IsMetered = false;
            }
        }

        public string GetHostName()
        {
            var hostNames = NetworkInformation.GetHostNames();
            return hostNames.FirstOrDefault(name => name.Type == HostNameType.DomainName)?.DisplayName;
        }
    }
}
