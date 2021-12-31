using System;

namespace Ghostly.Domain.Messages
{
    public sealed class RuleCreated
    {
        public Rule Rule { get; }

        public RuleCreated(Rule rule)
        {
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        }
    }
}
