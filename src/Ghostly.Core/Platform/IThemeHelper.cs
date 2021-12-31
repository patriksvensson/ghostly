using System.Threading.Tasks;

namespace Ghostly.Core.Pal
{
    public interface IThemeHelper
    {
        Task SetRequestedTheme();

        Theme GetTheme();
        Task SetTheme(Theme theme);
    }

    public enum Theme
    {
        Default = 0,
        Light = 1,
        Dark = 2,
    }
}
