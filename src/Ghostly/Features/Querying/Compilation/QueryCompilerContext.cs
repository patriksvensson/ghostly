using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Ghostly.Data.Models;

namespace Ghostly.Features.Querying.Compilation
{
    internal sealed class QueryCompilerContext
    {
        private readonly ContextStack<Type> _types;
        private readonly ContextStack<ParameterExpression> _parameters;

        public ParameterExpression Parameter => _parameters.Current;
        public bool ContainsError { get; set; }

        private class ContextStack<T>
        {
            private readonly T _default;
            private readonly Stack<T> _stack;

            public T Current => _stack.Count > 0 ? _stack.Peek() : _default;

            public ContextStack(T @default)
            {
                _default = @default;
                _stack = new Stack<T>();
            }

            public void Push(T item)
            {
                _stack.Push(item);
            }

            public void Pop()
            {
                if (_stack.Count > 0)
                {
                    _stack.Pop();
                }
            }
        }

        public QueryCompilerContext()
        {
            _types = new ContextStack<Type>(typeof(NotificationData));
            _parameters = new ContextStack<ParameterExpression>(Expression.Parameter(typeof(NotificationData)));
        }

        public void PushParameter(Type type)
        {
            _types.Push(type);
            _parameters.Push(Expression.Parameter(type));
        }

        public void PopParameter()
        {
            _types.Pop();
            _parameters.Pop();
        }

        public Expression MakeMemberAccess(IReadOnlyList<PropertyInfo> path)
        {
            var parameter = Parameter;

            var properties = new Queue<PropertyInfo>(path);
            var current = Expression.MakeMemberAccess(parameter, properties.Dequeue());
            while (properties.Count > 0)
            {
                current = Expression.MakeMemberAccess(current, properties.Dequeue());
            }

            return current;
        }
    }
}
