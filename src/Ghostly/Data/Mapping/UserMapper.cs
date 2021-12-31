using System;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;

namespace Ghostly.Data.Mapping
{
    public static class UserMapper
    {
        public static User Map(UserData data)
        {
            if (data == null)
            {
                return null;
            }

            if (data.Discriminator == Discriminator.GitHub)
            {
                return new GitHubUser
                {
                    Id = data.Id,
                    GitHubId = data.GitHubId.Value,
                    AvatarUrl = data.AvatarUrl,
                    AvatarHash = data.AvatarHash,
                    Username = data.Login,
                    Name = data.Name,
                };
            }

            throw new InvalidOperationException("Do not know how to map user.");
        }
    }
}
