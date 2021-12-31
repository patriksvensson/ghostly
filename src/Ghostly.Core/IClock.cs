using System;

namespace Ghostly
{
    public interface IClock
    {
        DateTimeOffset Now();
    }

    public sealed class Clock : IClock
    {
        public DateTimeOffset Now()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
