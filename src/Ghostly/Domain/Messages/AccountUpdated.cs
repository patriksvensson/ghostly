namespace Ghostly.Domain.Messages
{
    public sealed class AccountUpdated
    {
        public Account Account { get; }

        public AccountUpdated(Account account)
        {
            Account = account;
        }
    }
}
