using System.ComponentModel.Design;
using System.Reflection;

namespace Ghostly.Tools.Resources
{
    public class StringTypeResolutionService : ITypeResolutionService
    {
        public Assembly GetAssembly(AssemblyName name)
        {
            throw new InvalidOperationException();
        }

        public Assembly GetAssembly(AssemblyName name, bool throwOnError)
        {
            throw new InvalidOperationException();
        }

        public string GetPathOfAssembly(AssemblyName name)
        {
            throw new InvalidOperationException();
        }

        public Type GetType(string name)
        {
            return typeof(string);
        }

        public Type GetType(string name, bool throwOnError)
        {
            return typeof(string);
        }

        public Type GetType(string name, bool throwOnError, bool ignoreCase)
        {
            return typeof(string);
        }

        public void ReferenceAssembly(AssemblyName name)
        {
        }
    }
}
