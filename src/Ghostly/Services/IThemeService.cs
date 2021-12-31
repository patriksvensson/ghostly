using System.Threading.Tasks;
using Ghostly.Core;

namespace Ghostly.Services
{
    public interface IThemeService
    {
        GhostlyTheme Current { get; }
        GhostlyTheme Canonical { get; }

        Task InitializeTheme();
        Task SetTheme(GhostlyTheme theme);
    }
}
