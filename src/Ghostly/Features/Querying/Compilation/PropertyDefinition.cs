using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation
{
    internal abstract class PropertyDefinition
    {
        public IReadOnlyList<string> Names { get; }
        public abstract Type ResultType { get; }
        public virtual string Glyph => "\uE943";
        public virtual string LocalizedDescription => null;
        public virtual string LocalizedType { get; }
        public virtual bool ShowInAutoComplete => true;

        protected PropertyDefinition(string name, params string[] aliases)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Names = new List<string>(new[] { name }.Concat(aliases));
        }

        public abstract Expression CompileMember(QueryCompilerContext context);
    }

    internal abstract class PropertyDefinition<T> : PropertyDefinition
    {
        public sealed override Type ResultType => typeof(T);

        protected PropertyDefinition(string name, params string[] aliases)
            : base(name, aliases)
        {
        }
    }
}
