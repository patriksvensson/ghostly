using System;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;

namespace Ghostly.Data.Mapping
{
    public static class MilestoneMapper
    {
        public static Milestone Map(MilestoneData data)
        {
            if (data == null)
            {
                return null;
            }

            if (data.Discriminator == Discriminator.GitHub)
            {
                return new GitHubMilestone
                {
                    Id = data.Id,
                    Name = data.Name,
                    GitHubId = data.GitHubId,
                };
            }

            throw new InvalidOperationException("Do not know how to map milestone.");
        }
    }
}
