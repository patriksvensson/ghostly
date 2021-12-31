using Scriban.Runtime;

namespace Ghostly.Services.Templating
{
    public interface IScriptObjectImportable
    {
        void RegisterMethods(ScriptObject model);
    }
}
