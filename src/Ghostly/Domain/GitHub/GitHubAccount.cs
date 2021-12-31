using System;
using System.Linq;

namespace Ghostly.Domain.GitHub
{
    public sealed class GitHubAccount : Account
    {
        private string _description;

        public override Vendor VendorKind
        {
            get
            {
                return Vendor.GitHub;
            }
        }

        public override string DisplayName => Username;

        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public string[] Scopes { get; set; }
        public override string Description => _description;

        public override bool ImportEnabled => Scopes?.Contains("gist", StringComparer.OrdinalIgnoreCase) ?? false;
        public override bool ExportEnabled => Scopes?.Contains("gist", StringComparer.OrdinalIgnoreCase) ?? false;

        public override Uri Icon => Constants.GitHubIcon;

        public void SetDescription(string description)
        {
            _description = description ?? string.Empty;
        }
    }
}
