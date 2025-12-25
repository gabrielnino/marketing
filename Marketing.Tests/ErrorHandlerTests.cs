using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Result;
using Application.Result.Error;
using FluentAssertions;
using Infrastructure.Result;
using Xunit;

namespace Marketing.Tests;

public sealed class ErrorHandlerTests
{
    // ---------------------------
    // Helpers
    // ---------------------------

    private static string CreateTempMappingsFile(object mappings)
    {
        var path = Path.Combine(Path.GetTempPath(), $"ErrorMappings_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(mappings));
        return path;
    }

    private sealed class SpyErrorLogger : IErrorLogger
    {
        private readonly TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int CallCount { get; private set; }
        public Exception? LastException { get; private set; }
        public Task WhenCalled => _tcs.Task;

        public Task LogAsync(Exception ex, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastException = ex;
            _tcs.TrySetResult(true);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingErrorLogger : IErrorLogger
    {
        public Task LogAsync(Exception ex, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Logger failed");
    }

    // IMPORTANT:
    // ErrorHandler holds static mappings (Lazy<ConcurrentDictionary<...>>).
    // Tests must be isolated. We clear the static dictionary via reflection.
    private static void ResetStaticMappings()
    {
        var type = typeof(ErrorHandler);
        var field = type.GetField("ErrorMappings",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        field.Should().NotBeNull("ErrorMappings static field must exist for test isolation.");

        var lazy = field!.GetValue(null);
        lazy.Should().NotBeNull();

        var valueProp = lazy!.GetType().GetProperty("Value");
        valueProp.Should().NotBeNull();

        var dict = valueProp!.GetValue(lazy);
        dict.Should().NotBeNull();

        var clearMethod = dict!.GetType().GetMethod("Clear");
        clearMethod.Should().NotBeNull();

        clearMethod!.Invoke(dict, null);
    }

    private static ErrorHandler CreateSut(IErrorLogger logger)
        => new ErrorHandler(logger);

    // ---------------------------
    // Constructor
    // ---------------------------

    [Fact]
    public void Ctor_When_logger_is_null_should_throw()
    {
        var act = () => new ErrorHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ---------------------------
    // Fail<T> - null / not loaded / missing mapping
    // ---------------------------

    [Fact]
    public void Fail_When_exception_is_null_should_return_NullExceptionStrategy_failure()
    {
        ResetStaticMappings();

        var sut = CreateSut(new SpyErrorLogger());

        var op = sut.Fail<int>(null);

        op.IsSuccessful.Should().BeFalse();
        op.Type.Should().Be(ErrorTypes.NullExceptionStrategy);
        op.Message.Should().Be("Exception is null.");
    }

    [Fact]
    public void Fail_When_mappings_not_loaded_should_return_NullExceptionStrategy_failure()
    {
        ResetStaticMappings();

        var sut = CreateSut(new SpyErrorLogger());

        var op = sut.Fail<int>(new Exception("x"));

        op.IsSuccessful.Should().BeFalse();
        op.Type.Should().Be(ErrorTypes.NullExceptionStrategy);
        op.Message.Should().Be("ErrorMappings is not loaded or empty.");
    }

    [Fact]
    public void Fail_When_exception_type_not_in_mappings_should_return_NullExceptionStrategy_failure()
    {
        ResetStaticMappings();

        var logger = new SpyErrorLogger();
        var sut = CreateSut(logger);

        // Load mappings with a file that has at least one entry so LoadErrorMappings succeeds.
        var file = CreateTempMappingsFile(new { CustomException = "UnexpectedErrorStrategy" });
        sut.LoadErrorMappings(file);

        var op = sut.Fail<int>(new InvalidOperationException("not mapped"));

        op.IsSuccessful.Should().BeFalse();
        op.Type.Should().Be(ErrorTypes.NullExceptionStrategy);
        op.Message.Should().StartWith("No strategy matches exception type:");
        logger.CallCount.Should().Be(0, "logging should not occur when no strategy matches");
    }

    // ---------------------------
    // Fail<T> - mapped exception -> strategy
    // ---------------------------

    [Fact]
    public async Task Fail_When_mapped_and_no_custom_message_should_use_strategy_description_and_log()
    {
        ResetStaticMappings();

        var logger = new SpyErrorLogger();
        var sut = CreateSut(logger);

        // Ensure default mappings are loaded (LoadErrorMappings always seeds DefaultMappings first)
        var file = CreateTempMappingsFile(new { CustomException = "UnexpectedErrorStrategy" });
        sut.LoadErrorMappings(file);

        var ex = new HttpRequestException("network");
        var op = sut.Fail<int>(ex);

        op.IsSuccessful.Should().BeFalse();
        op.Type.Should().Be(ErrorTypes.Network);

        // For no custom message, ErrorStrategyBase.CreateFailure() uses ErrorTypes.GetDescription().
        // ErrorTypes.Network description is defined via EnumMetadata.
        op.Message.Should().Be("Occurs due to a network connectivity issue.");

        await logger.WhenCalled.WaitAsync(TimeSpan.FromSeconds(2));
        logger.CallCount.Should().Be(1);
        logger.LastException.Should().BeSameAs(ex);
    }

    [Fact]
    public async Task Fail_When_mapped_and_custom_message_should_use_custom_message_and_log()
    {
        ResetStaticMappings();

        var logger = new SpyErrorLogger();
        var sut = CreateSut(logger);

        var file = CreateTempMappingsFile(new { CustomException = "UnexpectedErrorStrategy" });
        sut.LoadErrorMappings(file);

        var ex = new JsonException("bad json");
        var op = sut.Fail<int>(ex, "payload invalid");

        op.IsSuccessful.Should().BeFalse();
        op.Type.Should().Be(ErrorTypes.InvalidData);
        op.Message.Should().Be("payload invalid");

        await logger.WhenCalled.WaitAsync(TimeSpan.FromSeconds(2));
        logger.CallCount.Should().Be(1);
        logger.LastException.Should().BeSameAs(ex);
    }

    [Fact]
    public void Fail_When_logger_throws_should_still_return_operation()
    {
        ResetStaticMappings();

        var sut = CreateSut(new ThrowingErrorLogger());

        var file = CreateTempMappingsFile(new { CustomException = "UnexpectedErrorStrategy" });
        sut.LoadErrorMappings(file);

        var ex = new HttpRequestException("network");
        var op = sut.Fail<int>(ex);

        op.IsSuccessful.Should().BeFalse();
        op.Type.Should().Be(ErrorTypes.Network);
        // No exception should escape even if logger fails
    }

    [Fact]
    public void Fail_When_mapping_points_to_unknown_strategy_should_fallback_to_Unexpected()
    {
        ResetStaticMappings();

        var logger = new SpyErrorLogger();
        var sut = CreateSut(logger);

        // Map InvalidOperationException to an unknown strategy name
        var file = CreateTempMappingsFile(new { InvalidOperationException = "DoesNotExistStrategy" });
        sut.LoadErrorMappings(file);

        var op = sut.Fail<int>(new InvalidOperationException("x"));

        op.IsSuccessful.Should().BeFalse();
        op.Type.Should().Be(ErrorTypes.Unexpected);
        op.Message.Should().Be("Occurs for any unexpected or unclassified error.");
    }

    // ---------------------------
    // Business<T>
    // ---------------------------

    [Fact]
    public void Business_Should_return_BusinessValidation_failure_with_message()
    {
        ResetStaticMappings();

        var sut = CreateSut(new SpyErrorLogger());

        var op = sut.Business<int>("invalid input");

        op.IsSuccessful.Should().BeFalse();
        op.Type.Should().Be(ErrorTypes.BusinessValidation);
        op.Message.Should().Be("invalid input");
    }

    // ---------------------------
    // LoadErrorMappings
    // ---------------------------

    [Fact]
    public void LoadErrorMappings_When_file_missing_should_throw_FileNotFoundException()
    {
        ResetStaticMappings();

        var sut = CreateSut(new SpyErrorLogger());

        var act = () => sut.LoadErrorMappings(Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.json"));

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void LoadErrorMappings_When_json_invalid_should_throw_InvalidOperationException_with_inner_JsonException()
    {
        ResetStaticMappings();

        var sut = CreateSut(new SpyErrorLogger());

        var path = Path.Combine(Path.GetTempPath(), $"badjson_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, "{ not valid json }");

        var act = () => sut.LoadErrorMappings(path);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ErrorMappings.json contains invalid JSON format.")
            .WithInnerException<JsonException>();
    }

    [Fact]
    public void LoadErrorMappings_When_json_empty_object_should_throw_wrapped_exception()
    {
        ResetStaticMappings();

        var sut = CreateSut(new SpyErrorLogger());
        var file = CreateTempMappingsFile(new { }); // {} => invalid

        Action act = () => sut.LoadErrorMappings(file);

        act.Should()
            .Throw<Exception>()
            .WithMessage("An error occurred while loading error mappings.")
            .WithInnerException<InvalidOperationException>()
            .Which.Message.Should().Be("ErrorMappings.json is empty or invalid.");
    }

    [Fact]
    public void LoadErrorMappings_When_file_does_not_exist_should_throw_FileNotFoundException()
    {
        ResetStaticMappings();

        var sut = CreateSut(new SpyErrorLogger());

        var missingFile = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.json");

        Action act = () => sut.LoadErrorMappings(missingFile);

        act.Should()
            .Throw<FileNotFoundException>()
            .WithMessage($"Error mappings file not found: {missingFile}");
    }
    //Error mappings file not found: C:\Users\luisg\AppData\Local\Temp\missing_2adc19af1a774bcdbeef22982dbb9415.json

    [Fact]
    public void Any_Should_be_false_before_load_and_true_after_successful_load()
    {
        ResetStaticMappings();

        var sut = CreateSut(new SpyErrorLogger());

        sut.Any().Should().BeFalse();

        var file = CreateTempMappingsFile(new { CustomException = "UnexpectedErrorStrategy" });
        sut.LoadErrorMappings(file);

        sut.Any().Should().BeTrue();
    }
}
