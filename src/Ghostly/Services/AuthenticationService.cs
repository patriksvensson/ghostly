using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Domain;

namespace Ghostly.Services
{
    public interface IAuthenticationService
    {
        Task<bool> CanLogin(Vendor vendorKind);
        Task OpenInBrowser(Account account);
        Task Login(Vendor type);
        Task Logout(Account account);
    }

    public sealed class AuthenticationService : IAuthenticationService
    {
        private readonly IReadOnlyList<IVendor> _vendors;

        public AuthenticationService(IEnumerable<IVendor> providers)
        {
            _vendors = new List<IVendor>(providers ?? Array.Empty<IVendor>());
        }

        public async Task<bool> CanLogin(Vendor vendorKind)
        {
            var provider = _vendors.Single(p => p.Matches(vendorKind));
            return await provider.CanLogin();
        }

        public async Task OpenInBrowser(Account account)
        {
            var provider = _vendors.Single(p => p.Matches(account.VendorKind));
            await provider.OpenInBrowser(account);
        }

        public async Task Login(Vendor vendorKind)
        {
            var provider = _vendors.Single(p => p.Matches(vendorKind));
            await provider.Login(vendorKind);
        }

        public async Task Logout(Account account)
        {
            var provider = _vendors.Single(p => p.Matches(account.VendorKind));
            await provider.Logout(account);
        }
    }
}
