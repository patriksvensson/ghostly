using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Ghostly.Data.Models;

namespace Ghostly.Features.Querying.Compilation
{
    internal abstract class CollectionDefinition : PropertyDefinition
    {
        public abstract Type CollectionType { get; }

        protected CollectionDefinition(string name, params string[] aliases)
            : base(name, aliases)
        {
        }

        public abstract Expression CompileCollectionMember(QueryCompilerContext context);
    }

    internal abstract class CollectionDefinition<TCollectionType, TResult> : CollectionDefinition
    {
        public sealed override Type CollectionType => typeof(TCollectionType);
        public sealed override Type ResultType => typeof(TResult);

        protected CollectionDefinition(string name, params string[] aliases)
            : base(name, aliases)
        {
        }

        public sealed override Expression CompileCollectionMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(GetCollection()));
        }

        public sealed override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(GetMember()));
        }

        protected abstract Expression<Func<NotificationData, List<TCollectionType>>> GetCollection();
        protected abstract Expression<Func<TCollectionType, TResult>> GetMember();
    }
}
