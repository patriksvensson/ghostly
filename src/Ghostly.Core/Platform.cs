using Ghostly.Core.Pal;
using Ghostly.Core.Threading;

namespace Ghostly.Core
{
    public static class Platform
    {
        public static IThreadingModel ThreadingModel { get; set; } = new DefaultThreadingModel();
    }
}
