using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace Ghostly.GitHub.Tests
{
    // Needs to be in this assembly to work...
    public sealed class EmbeddedResourceDataAttribute : DataAttribute
    {
        private readonly string[] _args;

        public EmbeddedResourceDataAttribute(params string[] args)
        {
            _args = args;
        }

        public static Stream GetManifestStream(string resourceName)
        {
            if (resourceName is null)
            {
                throw new ArgumentNullException(nameof(resourceName));
            }

            var assembly = typeof(EmbeddedResourceDataAttribute).Assembly;
            resourceName = resourceName.Replace("/", ".", StringComparison.Ordinal);
            return assembly.GetManifestResourceStream(resourceName);
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var result = new object[_args.Length];
            for (var index = 0; index < _args.Length; index++)
            {
                using (var stream = GetManifestStream(_args[index]))
                using (var reader = new StreamReader(stream))
                {
                    result[index] = reader.ReadToEnd();
                }
            }

            return new[] { result };
        }
    }
}
