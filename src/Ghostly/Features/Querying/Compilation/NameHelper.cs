using System;
using Ghostly.Features.Querying.Expressions.Ast;

namespace Ghostly.Features.Querying.Compilation
{
    internal static class NameHelper
    {
        public static string GetFriendlyName(Type type, bool capitalize = false)
        {
            if (type == typeof(bool))
            {
                return capitalize ? "Boolean" : "boolean";
            }

            if (type == typeof(string))
            {
                return capitalize ? "String" : "string";
            }

            if (type == typeof(int))
            {
                return capitalize ? "Integer" : "integer";
            }

            return capitalize ? "Unknown" : "unknown";
        }

        public static string GetFriendlyName(ComparisonOperator @operator)
        {
            switch (@operator)
            {
                case ComparisonOperator.LessThan:
                    return "Less than";
                case ComparisonOperator.LessThanOrEquals:
                    return "Less than or equal";
                case ComparisonOperator.GreaterThan:
                    return "Greater than";
                case ComparisonOperator.GreaterThanOrEquals:
                    return "Greater than or equal";
                case ComparisonOperator.Equals:
                    return "Equals";
                case ComparisonOperator.NotEquals:
                    return "Not equals";
                case ComparisonOperator.Contains:
                    return "Contains";
                default:
                    return "Unknown";
            }
        }
    }
}
