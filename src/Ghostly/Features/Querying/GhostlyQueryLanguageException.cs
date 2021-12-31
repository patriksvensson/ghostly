using System;

namespace Ghostly.Features.Querying
{
    public sealed class GhostlyQueryLanguageException : Exception
    {
        public GhostlyQueryLanguageException()
        {
        }

        public GhostlyQueryLanguageException(string message)
            : base(message)
        {
        }

        public GhostlyQueryLanguageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
