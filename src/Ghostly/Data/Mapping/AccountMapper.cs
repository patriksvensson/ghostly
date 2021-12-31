using System;
using System.Collections.Generic;
using System.Linq;
using Ghostly.Core.Services;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;

namespace Ghostly.Data.Mapping
{
    public static class AccountMapper
    {
        public static List<Account> Map(IEnumerable<AccountData> data, ILocalizer localizer)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            var result = new List<Account>();
            foreach (var item in data)
            {
                result.Add(Map(item, localizer));
            }

            return result;
        }

        public static Account Map(AccountData data, ILocalizer localizer)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            if (data.Discriminator == Discriminator.GitHub)
            {
                var account = new GitHubAccount
                {
                    Id = data.Id,
                    LastSyncedAt = data.LastSyncedAt?.EnsureUniversalTime(),
                    State = data.State,
                    AvatarUrl = data.AvatarUrl,
                    Scopes = data?.Scopes.Split(';') ?? Array.Empty<string>(),
                    Username = data.Username,
                };

                account.SetDescription(GetDescription(localizer, account));

                return account;
            }

            throw new InvalidOperationException("Do not know how to map account.");
        }

        public static AccountData Map(Account account)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if (account is GitHubAccount github)
            {
                return Map(github);
            }

            throw new InvalidOperationException("Do not know how to map account.");
        }

        public static AccountData Map(GitHubAccount account)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            return new AccountData
            {
                Id = account.Id,
                Discriminator = Discriminator.GitHub,
                LastSyncedAt = account.LastSyncedAt,
                State = account.State,
                AvatarUrl = account.AvatarUrl,
                Scopes = string.Join(";", account.Scopes),
                Username = account.Username,
            };
        }

        public static string GetDescription(ILocalizer localizer, Account account)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if (account is GitHubAccount github)
            {
                if (github?.Scopes?.Contains("repo") ?? false)
                {
                    return localizer["Accounts_PublicAndPrivateRepositories"];
                }
                else
                {
                    return localizer["Accounts_PublicRepositories"];
                }
            }

            return "?";
        }
    }
}
