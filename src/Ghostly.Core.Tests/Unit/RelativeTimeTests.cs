using System;
using Shouldly;
using Xunit;

namespace Ghostly.Core.Tests.Unit
{
    public sealed class RelativeTimeTests
    {
        public sealed class FakeClock : IClock
        {
            private readonly DateTime _now;

            public FakeClock(DateTime now)
            {
                _now = now.EnsureUniversalTime();
            }

            public DateTimeOffset Now()
            {
                return _now;
            }
        }

        [Theory]
        [InlineData(0, 0, 0, 0, 1, RelativeTimeUnit.Second, 1)]
        [InlineData(0, 0, 0, 0, 30, RelativeTimeUnit.Second, 30)]
        [InlineData(0, 0, 0, 0, 59, RelativeTimeUnit.Second, 59)]
        [InlineData(0, 0, 0, 1, 0, RelativeTimeUnit.Minute, 1)]
        [InlineData(0, 0, 0, 30, 0, RelativeTimeUnit.Minute, 30)]
        [InlineData(0, 0, 0, 45, 0, RelativeTimeUnit.Minute, 45)]
        [InlineData(0, 0, 1, 0, 0, RelativeTimeUnit.Hour, 1)]
        [InlineData(0, 0, 12, 0, 0, RelativeTimeUnit.Hour, 12)]
        [InlineData(0, 0, 23, 0, 0, RelativeTimeUnit.Hour, 23)]
        [InlineData(0, 1, 0, 0, 0, RelativeTimeUnit.Day, 1)]
        [InlineData(0, 2, 0, 0, 0, RelativeTimeUnit.Day, 2)]
        [InlineData(0, 4, 0, 0, 0, RelativeTimeUnit.Day, 4)]
        [InlineData(0, 15, 0, 0, 0, RelativeTimeUnit.Day, 15)]
        [InlineData(0, 30, 0, 0, 0, RelativeTimeUnit.Month, 1)]
        [InlineData(6, 0, 0, 0, 0, RelativeTimeUnit.Month, 6)]
        [InlineData(11, 0, 0, 0, 0, RelativeTimeUnit.Month, 11)]
        [InlineData(12, 0, 0, 0, 0, RelativeTimeUnit.Year, 1)]
        [InlineData(24, 0, 0, 0, 0, RelativeTimeUnit.Year, 2)]
        public void Should_Return_Correct_Relative_Time_For_Seconds(int months, int days, int hours, int minutes, int seconds, RelativeTimeUnit unit, int quantity)
        {
            // Given
            var clock = new FakeClock(new DateTime(2019, 1, 1, 0, 0, 0)
                .AddMonths(months)
                .AddDays(days)
                .AddHours(hours)
                .AddMinutes(minutes)
                .AddSeconds(seconds));

            // When
            var result = RelativeTime.GetRelativeTime(clock, new DateTime(2019, 1, 1, 0, 0, 0));

            // Then
            result.Unit.ShouldBe(unit);
            result.Quantity.ShouldBe(quantity);
        }
    }
}
