using System;
using System.Collections.Generic;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;

namespace Ghostly.Data.Mapping
{
    public static class CommentMapper
    {
        public static List<Comment> Map(IEnumerable<CommentData> data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var result = new List<Comment>();
            foreach (var item in data)
            {
                result.Add(Map(item));
            }

            return result;
        }

        public static Comment Map(CommentData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Discriminator == Discriminator.GitHub)
            {
                return new GitHubComment
                {
                    Id = data.Id,
                    GitHubId = data.GitHubId.Value,
                    Body = data.Body,
                    CreatedAt = data.CreatedAt.Value.EnsureUniversalTime(),
                    UpdatedAt = data.UpdatedAt?.EnsureUniversalTime(),
                    Url = new Uri(data.Url, UriKind.RelativeOrAbsolute),
                    Author = UserMapper.Map(data.Author),
                };
            }

            throw new InvalidOperationException("Do not know how to map comment.");
        }
    }
}
