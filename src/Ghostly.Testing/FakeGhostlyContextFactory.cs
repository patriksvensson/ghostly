using System;
using System.Globalization;
using Ghostly.Data;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Testing
{
    public sealed class FakeGhostlyContextFactory : IGhostlyContextFactory
    {
        private readonly string _name;

        public FakeGhostlyContextFactory(string name = null)
        {
            _name = name ?? Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        }

        public GhostlyContext Create()
        {
            return new GhostlyContext(new DbContextOptionsBuilder<GhostlyContext>()
                .UseInMemoryDatabase(databaseName: _name).Options);
        }
    }
}
