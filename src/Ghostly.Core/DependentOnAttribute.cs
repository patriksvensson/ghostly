using System;

namespace Ghostly.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class DependentOnAttribute : Attribute
    {
        public Type DependentOn { get; }

        public DependentOnAttribute(Type dependentOn)
        {
            DependentOn = dependentOn;
        }
    }
}
