using System;
using System.Threading.Tasks;

namespace Ghostly.Core.Pal
{
    public interface IUriLauncher
    {
        Task Launch(Uri uri);
    }
}
