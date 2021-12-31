using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ghostly.Domain
{
    public interface IVendorProfiles
    {
        Task<IReadOnlyList<SettingsProfile>> GetProfiles(Account model);
        Task<bool> Export(Account account, SettingsProfile profile);
    }
}
