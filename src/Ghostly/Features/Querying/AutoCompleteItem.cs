using System;
using System.Collections.Generic;
using Ghostly.Features.Querying.Compilation;

namespace Ghostly.Features.Querying
{
    public sealed class AutoCompleteItem
    {
        public string Name { get; set; }
        public string Glyph { get; set; }
        public AutoCompleteKind Kind { get; set; }
        public string LocalizedDescription { get; set; }
        public string Description { get; set; }
        public Type ReturnType { get; set; }

        internal static IEnumerable<AutoCompleteItem> CreateKeywords(params string[] names)
        {
            foreach (var name in names)
            {
                yield return new AutoCompleteItem
                {
                    Name = name,
                    Kind = AutoCompleteKind.Keyword,
                    Glyph = "\uE712",
                    LocalizedDescription = "Query_Keyword",
                };
            }
        }

        internal static IEnumerable<AutoCompleteItem> CreateConstants(params string[] names)
        {
            foreach (var name in names)
            {
                yield return new AutoCompleteItem
                {
                    Name = name,
                    Kind = AutoCompleteKind.Constant,
                    Glyph = null,
                    LocalizedDescription = "Query_Constant",
                };
            }
        }

        internal static IEnumerable<AutoCompleteItem> CreateOperators(params string[] names)
        {
            foreach (var name in names)
            {
                yield return new AutoCompleteItem
                {
                    Name = name,
                    Kind = AutoCompleteKind.Operator,
                    Glyph = null,
                    LocalizedDescription = "Query_Operator",
                };
            }
        }

        internal static IEnumerable<AutoCompleteItem> CreateProperties(PropertyDefinition definition)
        {
            foreach (var name in definition.Names)
            {
                yield return new AutoCompleteItem
                {
                    Name = name,
                    Kind = AutoCompleteKind.Property,
                    Glyph = definition.Glyph,
                    LocalizedDescription = GetLocalizedType(definition),
                    ReturnType = definition.ResultType,
                };
            }
        }

        private static string GetLocalizedType(PropertyDefinition definition)
        {
            if (!string.IsNullOrWhiteSpace(definition.LocalizedType))
            {
                return definition.LocalizedType;
            }

            if (definition.ResultType == typeof(bool))
            {
                return "Query_Type_Boolean";
            }

            if (definition.ResultType == typeof(string))
            {
                return "Query_Type_String";
            }

            if (definition.ResultType == typeof(int))
            {
                return "Query_Type_Integer";
            }

            return "Query_Type_Unknown";
        }
    }
}
