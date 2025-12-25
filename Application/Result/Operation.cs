namespace Application.Result
{
    using Application.Result.Error;
    using InvalidOperation = Exceptions.InvalidOperation;
    using static Application.Constants.Messages;

    /// <summary>
    /// Holds the result of an operation with its status, data, and error info.
    /// </summary>
    public class Operation<T> : Result<T>
    {
        private Operation() { }

        /// <summary>
        /// Build a successful operation, optionally with data and a message.
        /// </summary>
        public static Operation<T> Success(T? data, string? message = "")
        {
            return new Operation<T>
            {
                IsSuccessful = true,
                Data         = data,
                Message      = message ?? string.Empty,
                Type         = ErrorTypes.None
            };
        }

        /// <summary>
        /// Build a failed operation with an error message and type.
        /// </summary>
        public static Operation<T> Failure(string message, ErrorTypes errorTypes)
        {
            return new Operation<T>
            {
                IsSuccessful = false,
                Message      = message,
                Type         = errorTypes
            };
        }

        /// <summary>
        /// Convert this failure result into another generic type.
        /// </summary>
        public Operation<U> AsType<U>()
        {
            EnsureIsFailure();
            return new Operation<U>
            {
                IsSuccessful = false,
                Message      = this.Message,
                Type         = this.Type
            };
        }

        /// <summary>
        /// Alias for AsType to change the result type.
        /// </summary>
        public Operation<U> ConvertTo<U>() => AsType<U>();

        /// <summary>
        /// Ensure this instance is a failure; throws if it succeeded.
        /// </summary>
        /// <exception cref="InvalidOperation">
        /// Thrown when the operation is marked successful.
        /// </exception>
        private void EnsureIsFailure()
        {
            if (IsSuccessful)
            {
                throw new InvalidOperation(Operation.InvalidOperation);
            }
        }
    }
}
