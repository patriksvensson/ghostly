using System;

namespace Ghostly.Domain.Messages
{
    public sealed class RuleUpdated
    {
        public Rule Rule { get; }

        public RuleUpdated(Rule rule)
        {
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        }
    }
}
