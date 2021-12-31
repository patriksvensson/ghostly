using System.Collections.Generic;
using Ghostly.Domain;

namespace Ghostly.Services
{
    public interface ICultureService
    {
        Culture Current { get; }

        IReadOnlyList<Culture> GetSupportedCultures();
        void SetCulture(Culture language);
    }
}
