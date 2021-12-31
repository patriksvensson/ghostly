using System.Collections.Generic;

namespace Ghostly.Domain
{
    public sealed class RuleOrder
    {
        public int Id { get; }
        public int SortOrder { get; }

        public RuleOrder(int id, int sortorder)
        {
            Id = id;
            SortOrder = sortorder;
        }

        public static IReadOnlyList<RuleOrder> Create(IEnumerable<Rule> rules)
        {
            if (rules is null)
            {
                throw new System.ArgumentNullException(nameof(rules));
            }

            var index = 1;
            var ordering = new List<RuleOrder>();

            foreach (var rule in rules)
            {
                ordering.Add(new RuleOrder(rule.Id, index));
                index++;
            }

            return ordering;
        }
    }
}