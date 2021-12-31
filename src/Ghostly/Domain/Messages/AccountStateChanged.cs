using Ghostly.Data.Models;

namespace Ghostly.Domain.Messages
{
    public sealed class AccountStateChanged
    {
        public int AccountId { get; }
        public AccountState State { get; }

        public AccountStateChanged(int accountId, AccountState state)
        {
            AccountId = accountId;
            State = state;
        }
    }
}
