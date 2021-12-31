using System;
using System.Diagnostics.CodeAnalysis;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Windows.Security.Credentials;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    public sealed class UwpPasswordVault : IPasswordVault
    {
        private readonly PasswordVault _vault;
        private readonly IGhostlyLog _log;

        public UwpPasswordVault(IGhostlyLog log)
        {
            _vault = new PasswordVault();
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public string GetPassword(string resource, string username)
        {
            try
            {
                var token = _vault.Retrieve(resource, username);
                token.RetrievePassword();
                return token.Password;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Could not retrieve password for {PasswordResource}/{PasswordUsername}", resource, username);
                return null;
            }
        }

        public void SetPassword(string resource, string username, string password)
        {
            _vault.Add(new PasswordCredential(resource, username, password));
        }

        public void DeletePassword(string resource, string username, string password)
        {
            _vault.Remove(new PasswordCredential(resource, username, password));
        }
    }
}
