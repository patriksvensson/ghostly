using Ghostly.Services.Templating;
using Scriban.Runtime;

namespace Ghostly
{
    internal static class ScriptObjectExtensions
    {
        public static void ImportCustom(this ScriptObject model, IScriptObjectImportable importable)
        {
            if (model != null && importable != null)
            {
                model.Import(importable);
                importable.RegisterMethods(model);
            }
        }
    }
}
