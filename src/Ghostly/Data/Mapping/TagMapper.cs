using System;
using System.Collections.Generic;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;

namespace Ghostly.Data.Mapping
{
    public static class TagMapper
    {
        public static List<Tag> Map(IEnumerable<WorkItemTagData> data)
        {
            var result = new List<Tag>();
            if (data != null)
            {
                foreach (var item in data)
                {
                    result.Add(Map(item));
                }
            }

            return result;
        }

        public static Tag Map(WorkItemTagData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Tag.Discriminator == Discriminator.GitHub)
            {
                return new GitHubLabel
                {
                    Id = data.Tag.Id,
                    GitHubId = data.Tag.GitHubId.Value,
                    Name = data.Tag.Name,
                    Description = data.Tag.Description,
                    Color = data.Tag.Color,
                };
            }

            throw new InvalidOperationException("Do not know how to map tag.");
        }
    }
}
