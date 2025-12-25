using Application.Result.EnumType.Extensions;
using Application.Result.Error;

namespace Application.Result
{
    /// <summary>
    /// Defines how to create a failed operation for type T.
    /// </summary>
    public interface IErrorCreationStrategy<T>
    {
        /// <summary>
        /// Create a failure with a custom message.
        /// </summary>
        Operation<T> CreateFailure(string message);

        /// <summary>
        /// Create a failure using the default message.
        /// </summary>
        Operation<T> CreateFailure();
    }

    /// <summary>
    /// Base class for strategies that build failure results.
    /// </summary>
    public abstract class ErrorStrategyBase<T>(ErrorTypes errorType) : IErrorCreationStrategy<T>
    {
        private readonly ErrorTypes _errorType = errorType;

        /// <inheritdoc />
        public Operation<T> CreateFailure(string message)
            => Operation<T>.Failure(message, _errorType);

        /// <inheritdoc />
        public Operation<T> CreateFailure()
            => Operation<T>.Failure(_errorType.GetDescription(), _errorType);
    }

    /// <summary>Handles business validation errors.</summary>
    public class BusinessStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.BusinessValidation);

    /// <summary>Handles missing configuration errors.</summary>
    public class ConfigMissingStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.ConfigMissing);

    /// <summary>Handles database errors.</summary>
    public class DatabaseStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Database);

    /// <summary>Handles invalid data errors.</summary>
    public class InvalidDataStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.InvalidData);

    /// <summary>Handles unexpected errors.</summary>
    public class UnexpectedErrorStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Unexpected);

    /// <summary>Handles network errors.</summary>
    public class NetworkErrorStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Network);

    /// <summary>Handles null exception cases.</summary>
    public class NullExceptionStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.NullExceptionStrategy);

    /// <summary>Handles user input errors.</summary>
    public class UserInputStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.UserInput);

    /// <summary>Handles not-found errors.</summary>
    public class NotFoundStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.NotFound);

    /// <summary>Handles authentication errors.</summary>
    public class AuthenticationStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Authentication);

    /// <summary>Handles authorization errors.</summary>
    public class AuthorizationStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Authorization);

    /// <summary>Handles resource-related errors.</summary>
    public class ResourceStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Resource);

    /// <summary>Handles timeout errors.</summary>
    public class TimeoutStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Timeout);

    /// <summary>No error strategy (success).</summary>
    public class NoneStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.None);

    /// <summary>
    /// Chooses a strategy to create a failed operation.
    /// </summary>
    public static class OperationStrategy<T>
    {
        private const string DefaultErrorMessage = "Unknown Error";

        /// <summary>
        /// Create a failure using the given strategy.
        /// </summary>
        /// <param name="message">Optional custom message.</param>
        /// <param name="strategy">Strategy to apply.</param>
        public static Operation<T> Fail(string? message, IErrorCreationStrategy<T> strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy), "Strategy cannot be null.");

            var finalMessage = string.IsNullOrWhiteSpace(message)
                ? DefaultErrorMessage
                : message;

            return strategy.CreateFailure(finalMessage);
        }
    }
}
