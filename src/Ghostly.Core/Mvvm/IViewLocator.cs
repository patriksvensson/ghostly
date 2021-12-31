using System;

namespace Ghostly.Core.Mvvm
{
    public interface IViewLocator
    {
        Type Resolve(Type viewModelType);
    }
}
