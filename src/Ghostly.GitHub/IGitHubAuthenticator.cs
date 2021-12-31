using System.Threading.Tasks;
using Ghostly.Domain;

namespace Ghostly.GitHub
{
    public interface IGitHubAuthenticator
    {
        Task OpenInBrowser(Account account);
        Task Login();
        Task Authenticate(string code);
    }
}
