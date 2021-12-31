using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Ghostly.Data.Models;

namespace Ghostly.Features.Querying.Compilation
{
    internal static class ExpressionHelper
    {
        public static Expression CoerceTypes(Expression left, Expression right, Func<Expression, Expression, BinaryExpression> func)
        {
            bool IsNullableType(Type t)
            {
                return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
            }

            if (IsNullableType(left.Type) && !IsNullableType(right.Type))
            {
                right = Expression.Convert(right, left.Type);
            }
            else if (!IsNullableType(left.Type) && IsNullableType(right.Type))
            {
                left = Expression.Convert(left, right.Type);
            }

            return func(left, right);
        }

        public static IReadOnlyList<PropertyInfo> GetPropertyPath<TResult>(Expression<Func<NotificationData, TResult>> expression)
        {
            return GetPropertyPath<NotificationData, TResult>(expression);
        }

        public static IReadOnlyList<PropertyInfo> GetPropertyPath<TEntity, TResult>(Expression<Func<TEntity, TResult>> expression)
        {
            if (!(expression.Body is MemberExpression member))
            {
                throw new InvalidOperationException("Expected member expression");
            }

            var properties = GetPropertyPath(member);
            if (properties.Count == 0)
            {
                throw new InvalidOperationException("Could not resolve property from expression.");
            }

            return properties;
        }

        private static IReadOnlyList<PropertyInfo> GetPropertyPath(MemberExpression member)
        {
            var parts = new Stack<PropertyInfo>();
            while (member?.Expression != null)
            {
                if (!(member.Member is PropertyInfo property))
                {
                    throw new InvalidOperationException("Only properties can be mapped.");
                }

                parts.Push(property);
                member = member.Expression as MemberExpression;
            }

            return parts.ToArray();
        }
    }
}
