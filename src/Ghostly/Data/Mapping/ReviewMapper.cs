using System;
using System.Collections.Generic;
using Ghostly.Data.Models;
using Ghostly.Domain;

namespace Ghostly.Data.Mapping
{
    public sealed class ReviewMapper
    {
        public static List<Review> Map(IEnumerable<ReviewData> data)
        {
            var result = new List<Review>();
            if (data != null)
            {
                foreach (var review in data)
                {
                    result.Add(Map(review));
                }
            }

            return result;
        }

        public static Review Map(ReviewData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return new Review
            {
                Id = data.Id,
                GitHubId = data.GitHubId,
                CreatedAt = data.CreatedAt.EnsureUniversalTime(),
                Author = UserMapper.Map(data.Author),
                State = data.State,
                Url = new Uri(data.Url, UriKind.RelativeOrAbsolute),
                Body = data.Body,
                Comments = Map(data.Comments),
            };
        }

        public static List<ReviewComment> Map(IEnumerable<ReviewCommentData> data)
        {
            var result = new List<ReviewComment>();
            if (data != null)
            {
                foreach (var comment in data)
                {
                    result.Add(Map(comment));
                }
            }

            return result;
        }

        public static ReviewComment Map(ReviewCommentData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return new ReviewComment
            {
                Id = data.Id,
                GitHubId = data.GitHubId,
                CreatedAt = data.CreatedAt.EnsureUniversalTime(),
                UpdatedAt = data.UpdatedAt?.EnsureUniversalTime(),
                OriginalCommitId = data.OriginalCommitId,
                CommitId = data.CommitId,
                Path = data.Path,
                Position = data.Position,
                OriginalPosition = data.Position,
                Diff = data.Diff,
                Url = new Uri(data.Url, UriKind.RelativeOrAbsolute),
                Body = data.Body,
                InReplyToId = data.InReplyToId,
                Author = UserMapper.Map(data.Author),
            };
        }
    }
}
