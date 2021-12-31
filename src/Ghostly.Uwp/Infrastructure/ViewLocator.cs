using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Mvvm;
using Ghostly.Data;
using Windows.UI.Xaml;

namespace Ghostly.Uwp.Infrastructure
{
    [DependentOn(typeof(DatabaseInitializer))]
    public sealed class ViewLocator : IViewLocator, IInitializable
    {
        private readonly Dictionary<string, Type> _lookup;

        public ViewLocator()
        {
            _lookup = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        }

        public Task<bool> Initialize(bool background)
        {
            Initialize(typeof(ViewLocator).Assembly);
            return Task.FromResult(true);
        }

        public Type Resolve(Type viewModelType)
        {
            if (viewModelType is null)
            {
                throw new ArgumentNullException(nameof(viewModelType));
            }

            _lookup.TryGetValue(viewModelType.Name, out var type);
            return type;
        }

        private void Initialize(Assembly assembly)
        {
            var baseType = typeof(UIElement);
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract &&
                    baseType.IsAssignableFrom(type) &&
                    type.Name.EndsWith("View", StringComparison.OrdinalIgnoreCase))
                {
                    _lookup.Add($"{type.Name}Model", type);
                }
            }
        }
    }
}
