using System.Reflection;

namespace Ghostly.Core
{
    public static class AssemblyExtensions
    {
        public static string GetAssemblyName(this Assembly assembly)
        {
            if (assembly is null)
            {
                throw new System.ArgumentNullException(nameof(assembly));
            }

            return assembly.FullName.Remove(assembly.FullName.IndexOf(','));
        }
    }
}
