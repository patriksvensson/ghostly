using System;
using System.Collections.Generic;
using Ghostly.Features.Querying.Compilation.Properties;

namespace Ghostly.Features.Querying.Compilation
{
    internal sealed class PropertyDefinitionCollection
    {
        private readonly IDictionary<string, PropertyDefinition> _lookup;
        private readonly List<PropertyDefinition> _compilers;

        public static PropertyDefinitionCollection Instance { get; } = new PropertyDefinitionCollection();

        public IReadOnlyCollection<PropertyDefinition> Definitions => _compilers;

        public PropertyDefinitionCollection()
        {
            _lookup = new Dictionary<string, PropertyDefinition>(StringComparer.OrdinalIgnoreCase);
            _compilers = new List<PropertyDefinition>
            {
                new ArchivedProperty(),
                new AssignedProperty(),
                new AuthorProperty(),
                new BodyProperty(),
                new CategoryProperty(),
                new ClosedProperty(),
                new CommenterProperty(),
                new CommentProperty(),
                new CommitProperty(),
                new DiscussionProperty(),
                new DraftProperty(),
                new ExternalIdentifierProperty(),
                new ForkProperty(),
                new InboxProperty(),
                new IssueProperty(),
                new LabelProperty(),
                new LockedProperty(),
                new MergedProperty(),
                new MergedByProperty(),
                new MilestoneProperty(),
                new MutedProperty(),
                new OpenProperty(),
                new OwnerProperty(),
                new PrivateProperty(),
                new PullRequestProperty(),
                new ReadProperty(),
                new ReleaseProperty(),
                new ReopenedProperty(),
                new RepositoryProperty(),
                new ReviewerProperty(),
                new ReviewCountProperty(),
                new StarredProperty(),
                new TitleProperty(),
                new UnreadProperty(),
                new VulnerabilityProperty(),
            };

            foreach (var compiler in _compilers)
            {
                foreach (var name in compiler.Names)
                {
                    if (_lookup.ContainsKey(name))
                    {
                        throw new InvalidOperationException($"Property {name} have already been registered.");
                    }

                    _lookup[name] = compiler;
                }
            }
        }

        public bool Exist(string property)
        {
            return _lookup.ContainsKey(property);
        }

        public bool TryGet(string property, out PropertyDefinition compiler)
        {
            return _lookup.TryGetValue(property, out compiler);
        }
    }
}
