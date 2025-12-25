using Application.Result;
using Application.Result.Error;
using FluentAssertions;
using Xunit;

namespace Marketing.Tests
{
    public class OperationTests
    {
        [Fact]
        public void Success_Should_set_success_fields()
        {
            var op = Operation<int>.Success(123, "ok");

            op.IsSuccessful.Should().BeTrue();
            op.Data.Should().Be(123);
            op.Message.Should().Be("ok");
            op.Type.Should().Be(ErrorTypes.None);
        }

        [Fact]
        public void Failure_Should_set_failure_fields()
        {
            var op = Operation<int>.Failure("boom", ErrorTypes.Unexpected);

            op.IsSuccessful.Should().BeFalse();
            op.Data.Should().Be(default);
            op.Message.Should().Be("boom");
            op.Type.Should().Be(ErrorTypes.Unexpected);
        }

        [Fact]
        public void AsType_When_success_should_throw()
        {
            var op = Operation<int>.Success(1, "ok");

            var act = () => op.AsType<string>();

            act.Should().Throw<Exception>(); // tighten to your InvalidOperation type after referencing it
        }

        [Fact]
        public void AsType_When_failure_should_map_message_and_type()
        {
            var op = Operation<int>.Failure("bad", ErrorTypes.BusinessValidation);

            var converted = op.AsType<string>();

            converted.IsSuccessful.Should().BeFalse();
            converted.Message.Should().Be("bad");
            converted.Type.Should().Be(ErrorTypes.BusinessValidation);
        }
    }
}