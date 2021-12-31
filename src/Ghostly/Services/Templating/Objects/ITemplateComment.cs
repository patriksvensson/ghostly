using Ghostly.Core;
using Ghostly.Domain;

namespace Ghostly.Services.Templating.Objects
{
    public interface ITemplateComment : IHaveTimestamp
    {
        User Author { get; }
        string Action { get; }
        string Url { get; }
        string Body { get; }
    }
}
