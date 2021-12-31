using System;
using Ghostly.Data.Models;
using Ghostly.Domain.GitHub;

namespace Ghostly.Data.Mapping
{
    public static class RepositoryMapper
    {
        public static GitHubRepository Map(RepositoryData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Discriminator == Discriminator.GitHub)
            {
                if (data.GitHubId == null)
                {
                    throw new ArgumentException("GitHub ID cannot be null", nameof(data));
                }

                return new GitHubRepository
                {
                    Id = data.Id,
                    GitHubId = data.GitHubId.Value,
                    Private = data.Private,
                    Fork = data.Fork,
                    Owner = data.Owner,
                    Name = data.Name,
                };
            }

            throw new InvalidOperationException("Do not know how to map repository.");
        }
    }
}
