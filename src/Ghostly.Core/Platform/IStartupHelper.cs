using System.Threading.Tasks;

namespace Ghostly.Core.Pal
{
    public interface IStartupHelper
    {
        Task<StartupState> GetState();
        Task<bool> Enable();
        Task<bool> Disable();
    }

    public enum StartupState
    {
        Error = 0,
        Enabled,
        EnabledByPolicy,
        Disabled,
        DisabledByUser,
        DisabledByPolicy,
    }
}
