using System.Threading.Tasks;

namespace Ghostly.Core.Pal
{
    public interface IPackageService
    {
        string GetName();
        string GetVersion();
        string GetFirstInstalledVersion();
    }
}
