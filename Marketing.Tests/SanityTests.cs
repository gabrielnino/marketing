using FluentAssertions;
using Xunit;

namespace Marketing.Tests
{
    public class SanityTests
    {
        [Fact]
        public void TestRunner_Works()
        {
            true.Should().BeTrue();
        }
    }
}