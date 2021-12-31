using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Ghostly.Data.Models;
using Ghostly.Features.Querying.Expressions;
using Ghostly.Features.Querying.Expressions.Ast;
using Microsoft.EntityFrameworkCore;
using ConstantExpression = Ghostly.Features.Querying.Expressions.Ast.ConstantExpression;

namespace Ghostly.Features.Querying.Compilation
{
    internal sealed class QueryCompiler : QueryVisitor<QueryCompilerContext, Expression>
    {
        public bool TryCompile(GhostlyExpression input, out Expression<Func<NotificationData, bool>> compiled, out string errorMessage)
        {
            var context = new QueryCompilerContext();

            try
            {
                compiled = Expression.Lambda<Func<NotificationData, bool>>(
                    input.Accept(this, context),
                    context.Parameter);

                errorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                compiled = null;
                errorMessage = ex.Message;
                return false;
            }
        }

        protected override Expression Visit(AndExpression expression, QueryCompilerContext context)
        {
            return Expression.AndAlso(
                expression.Left.Accept(this, context),
                expression.Right.Accept(this, context));
        }

        protected override Expression Visit(ComparisonExpression expression, QueryCompilerContext context)
        {
            // Is the left hand a collection?
            if (expression.Left is CollectionExpression property)
            {
                if (PropertyDefinitionCollection.Instance.TryGet(property.Name, out var definition) && definition is CollectionDefinition collection)
                {
                    // Get the parameter.
                    var parameter = expression.Left.Accept(this, context);

                    // Combine with a property accessor.
                    context.PushParameter(collection.CollectionType);
                    var predicate = Expression.Lambda(
                        new ComparisonExpression(expression.Operator,
                            PropertyExpression.Create(property.Name),
                            expression.Right).Accept(this, context),
                        context.Parameter);
                    context.PopParameter();

                    // Combine them into an Enumerable.Any or Enumerable.All call.
                    var method = expression.Operator == ComparisonOperator.NotEquals ? "All" : "Any";
                    return Expression.Call(typeof(Enumerable), method, new Type[] { collection.CollectionType }, parameter, predicate);
                }
                else
                {
                    throw new InvalidOperationException("Expected a collection definition.");
                }
            }

            var left = expression.Left.Accept(this, context);
            var right = GetRightHandSide(expression, context);

            // Contains?
            if (expression.Operator == ComparisonOperator.Contains)
            {
                if (right is System.Linq.Expressions.ConstantExpression constant && constant.Value is string stringValue)
                {
                    right = Expression.Constant($"%{stringValue}%");
                }

                return Expression.Call(
                    null, typeof(DbFunctionsExtensions).GetMethod("Like", new[] { typeof(DbFunctions), typeof(string), typeof(string) }),
                    Expression.Constant(EF.Functions), left, right);
            }

            switch (expression.Operator)
            {
                case ComparisonOperator.LessThan:
                    return ExpressionHelper.CoerceTypes(left, right, (l, r) => Expression.LessThan(l, r));
                case ComparisonOperator.LessThanOrEquals:
                    return ExpressionHelper.CoerceTypes(left, right, (l, r) => Expression.LessThanOrEqual(l, r));
                case ComparisonOperator.GreaterThan:
                    return ExpressionHelper.CoerceTypes(left, right, (l, r) => Expression.GreaterThan(l, r));
                case ComparisonOperator.GreaterThanOrEquals:
                    return ExpressionHelper.CoerceTypes(left, right, (l, r) => Expression.GreaterThanOrEqual(l, r));
                case ComparisonOperator.Equals:
                    return ExpressionHelper.CoerceTypes(left, right, (l, r) => Expression.Equal(l, r));
                case ComparisonOperator.NotEquals:
                    return ExpressionHelper.CoerceTypes(left, right, (l, r) => Expression.NotEqual(l, r));
            }

            throw new InvalidOperationException($"Unknown operator '{expression.Operator}'.");
        }

        private Expression GetRightHandSide(ComparisonExpression expression, QueryCompilerContext context)
        {
            // Is the left side a parameter and the right side a constant?
            if (expression.Left is PropertyExpression property && expression.Right is ConstantExpression constant)
            {
                if (!PropertyDefinitionCollection.Instance.TryGet(property.Name, out var definition))
                {
                    // Shouldn't happen since we visit the left side, but can never be safe enough :)
                    throw new InvalidOperationException($"Unknown property {property.Name}.");
                }

                // Wrong type?
                if (definition.ResultType != constant.Type)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.InvariantCulture, "Property '{0}' wanted a {1} but got a {2}.",
                            property.Name, NameHelper.GetFriendlyName(definition.ResultType),
                            NameHelper.GetFriendlyName(constant.Type)));
                }

                // Trying to use operators not defined?
                if (definition.ResultType != typeof(int)
                    && expression.Operator != ComparisonOperator.Equals
                    && expression.Operator != ComparisonOperator.NotEquals
                    && expression.Operator != ComparisonOperator.Contains)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.InvariantCulture, "You can't use the '{0}'-operator with a property that is a {1}.",
                            NameHelper.GetFriendlyName(expression.Operator),
                            NameHelper.GetFriendlyName(definition.ResultType)));
                }
            }

            return expression.Right.Accept(this, context);
        }

        protected override Expression Visit(ConstantExpression expression, QueryCompilerContext context)
        {
            return Expression.Constant(expression.Value);
        }

        protected override Expression Visit(StringLiteralExpression expression, QueryCompilerContext context)
        {
            return Expression.Constant(expression.Value);
        }

        protected override Expression Visit(NotExpression expression, QueryCompilerContext context)
        {
            return Expression.Not(expression.Operand.Accept(this, context));
        }

        protected override Expression Visit(OrExpression expression, QueryCompilerContext context)
        {
            return Expression.OrElse(
                expression.Left.Accept(this, context),
                expression.Right.Accept(this, context));
        }

        protected override Expression Visit(CollectionExpression expression, QueryCompilerContext context)
        {
            if (!PropertyDefinitionCollection.Instance.TryGet(expression.Name, out var compiler))
            {
                throw new InvalidOperationException($"Unknown property {expression.Name}.");
            }

            if (!(compiler is CollectionDefinition collectionCompiler))
            {
                throw new InvalidOperationException($"Property definition is not for a collection.");
            }

            return collectionCompiler.CompileCollectionMember(context);
        }

        protected override Expression Visit(PropertyExpression expression, QueryCompilerContext context)
        {
            if (!PropertyDefinitionCollection.Instance.TryGet(expression.Name, out var compiler))
            {
                throw new InvalidOperationException($"Unknown property {expression.Name}.");
            }

            return compiler.CompileMember(context);
        }
    }
}
