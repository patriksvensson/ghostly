namespace Ghostly.Domain.Messages
{
    public sealed class RuleDeleted
    {
        public int RuleId { get; set; }

        public RuleDeleted(int ruleId)
        {
            RuleId = ruleId;
        }
    }
}
