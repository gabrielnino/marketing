using System;
using Application.Result;
using Application.Result.Error;
using Application.Result.Exceptions;
using FluentAssertions;
using Xunit;

namespace Marketing.Tests;

public class OperationTests
{
    // ---------------------------
    // Success(...) invariants
    // ---------------------------

    [Fact]
    public void Success_When_message_is_null_should_normalize_to_empty_string()
    {
        var op = Operation<int>.Success(10, null);

        op.IsSuccessful.Should().BeTrue();
        op.Data.Should().Be(10);
        op.Message.Should().Be(string.Empty);
        op.Type.Should().Be(ErrorTypes.None);
    }

    [Fact]
    public void Success_When_message_is_omitted_should_default_to_empty_string()
    {
        var op = Operation<int>.Success(10);

        op.IsSuccessful.Should().BeTrue();
        op.Data.Should().Be(10);
        op.Message.Should().Be(string.Empty);
        op.Type.Should().Be(ErrorTypes.None);
    }

    [Fact]
    public void Success_When_data_is_null_should_still_be_success()
    {
        var op = Operation<string>.Success(null, "ok");

        op.IsSuccessful.Should().BeTrue();
        op.Data.Should().BeNull();
        op.Message.Should().Be("ok");
        op.Type.Should().Be(ErrorTypes.None);
    }

    [Fact]
    public void Success_Should_allow_empty_message()
    {
        var op = Operation<int>.Success(1, "");

        op.IsSuccessful.Should().BeTrue();
        op.Message.Should().Be(string.Empty);
        op.Type.Should().Be(ErrorTypes.None);
    }

    // ---------------------------
    // Failure(...) invariants
    // ---------------------------

    [Theory]
    [InlineData(ErrorTypes.Unexpected)]
    [InlineData(ErrorTypes.BusinessValidation)]
    [InlineData(ErrorTypes.Database)]
    public void Failure_Should_set_failure_fields(ErrorTypes type)
    {
        var op = Operation<int>.Failure("boom", type);

        op.IsSuccessful.Should().BeFalse();
        op.Data.Should().Be(default(int));
        op.Message.Should().Be("boom");
        op.Type.Should().Be(type);
    }

    [Fact]
    public void Failure_Should_allow_null_message_and_preserve_it()
    {
        // Current implementation does NOT normalize null on failure.
        // This test locks the behavior. If you want normalization later,
        // change the implementation and update this test accordingly.
        var op = Operation<int>.Failure(null!, ErrorTypes.Unexpected);

        op.IsSuccessful.Should().BeFalse();
        op.Message.Should().BeNull();
        op.Type.Should().Be(ErrorTypes.Unexpected);
    }

    // -----------------------------------------
    // AsType<U>() / ConvertTo<U>() guard logic
    // -----------------------------------------

    [Fact]
    public void AsType_When_called_on_success_should_throw_InvalidOperation_with_exact_message()
    {
        var op = Operation<int>.Success(1, "ok");

        var act = () => op.AsType<string>();

        act.Should()
           .Throw<InvalidOperation>()
           .WithMessage("This method can only be used if the value of IsSuccessful is false.");
    }

    [Fact]
    public void ConvertTo_When_called_on_success_should_throw_same_InvalidOperation()
    {
        var op = Operation<int>.Success(1);

        var act = () => op.ConvertTo<string>();

        act.Should()
           .Throw<InvalidOperation>()
           .WithMessage("This method can only be used if the value of IsSuccessful is false.");
    }

    [Fact]
    public void AsType_When_called_on_failure_should_preserve_message_and_type_and_return_failure()
    {
        var op = Operation<int>.Failure("bad", ErrorTypes.BusinessValidation);

        var converted = op.AsType<string>();

        converted.IsSuccessful.Should().BeFalse();
        converted.Data.Should().BeNull(); // new T is reference type; default is null
        converted.Message.Should().Be("bad");
        converted.Type.Should().Be(ErrorTypes.BusinessValidation);
    }

    [Fact]
    public void AsType_When_called_on_failure_should_work_for_value_types()
    {
        var op = Operation<string>.Failure("bad", ErrorTypes.InvalidData);

        var converted = op.AsType<int>();

        converted.IsSuccessful.Should().BeFalse();
        converted.Data.Should().Be(default(int)); // 0
        converted.Message.Should().Be("bad");
        converted.Type.Should().Be(ErrorTypes.InvalidData);
    }

    [Fact]
    public void ConvertTo_Should_be_equivalent_to_AsType_for_failures()
    {
        var op = Operation<Guid>.Failure("x", ErrorTypes.Network);

        var a = op.AsType<int>();
        var b = op.ConvertTo<int>();

        b.IsSuccessful.Should().Be(a.IsSuccessful);
        b.Type.Should().Be(a.Type);
        b.Message.Should().Be(a.Message);
        b.Data.Should().Be(a.Data);
    }

    [Fact]
    public void Multiple_conversions_should_be_stable_and_not_mutate_original()
    {
        var original = Operation<int>.Failure("bad", ErrorTypes.Timeout);

        var c1 = original.AsType<string>();
        var c2 = original.AsType<Guid>();

        // original unchanged
        original.IsSuccessful.Should().BeFalse();
        original.Message.Should().Be("bad");
        original.Type.Should().Be(ErrorTypes.Timeout);

        // conversions consistent
        c1.Message.Should().Be("bad");
        c1.Type.Should().Be(ErrorTypes.Timeout);

        c2.Message.Should().Be("bad");
        c2.Type.Should().Be(ErrorTypes.Timeout);
    }
}
