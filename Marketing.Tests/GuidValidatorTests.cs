using FluentAssertions;
using Infrastructure.Utilities;
using Xunit;

namespace Marketing.Tests
{
    public class GuidValidatorTests
    {
        [Theory]
        [InlineData("1b4e28ba-2fa1-11d2-883f-0016d3cca427")]
        [InlineData("6F9619FF-8B86-D011-B42D-00C04FC964FF")]
        public void HasGuid_When_valid_guid_returns_success(string id)
        {
            var op = GuidValidator.HasGuid(id);

            op.IsSuccessful.Should().BeTrue();
            op.Data.Should().Be(id);
            op.Message.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData("not-a-guid")]
        [InlineData("")]

        // NOTE: current implementation does Guid.TryParse only (no null/empty guard). Document expected behavior.
        public void HasGuid_When_invalid_returns_business_failure(string id)
        {
            var op = GuidValidator.HasGuid(id);

            op.IsSuccessful.Should().BeFalse();
            op.Data.Should().BeNull();
        }
    }
}