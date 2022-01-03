using System;
using System.Collections.Generic;
using System.Text;

namespace Ghostly.GitHub
{
    public partial class GitHubSecrets
    {
        public static GitHubSecrets Instance { get; } = new GitHubSecrets();

        public bool IsValid()
        {
            string value = null;
            GetClientId(ref value);
            if (value == null)
            {
                return false;
            }

            GetClientSecret(ref value);
            if (value == null)
            {
                return false;
            }

            return true;
        }

        public string ClientId
        {
            get
            {
                string value = null;
                GetClientId(ref value);
                if (value == null)
                {
                    throw new InvalidOperationException("No client ID specified");
                }

                return value;
            }
        }

        public string ClientSecret
        {
            get
            {
                string value = null;
                GetClientSecret(ref value);
                if (value == null)
                {
                    throw new InvalidOperationException("No client secret specified");
                }

                return value;
            }
        }

        partial void GetClientId(ref string value);
        partial void GetClientSecret(ref string value);
    }
}
