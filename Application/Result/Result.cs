using Application.Result.EnumType.Extensions;
using Application.Result.Error;

namespace Application.Result
{
    /// <summary>
    /// Holds the outcome of an operation.
    /// </summary>
    public class Result<T>
    {
        /// <summary>
        /// The error category, or None if successful.
        /// </summary>
        public ErrorTypes Type { get; set; }

        /// <summary>
        /// True when the operation completed without errors.
        /// </summary>
        public bool IsSuccessful { get; protected set; }

        /// <summary>
        /// The returned value when the operation succeeds.
        /// </summary>
        public T? Data { get; protected set; }

        /// <summary>
        /// A message describing the result or error.
        /// </summary>
        public string? Message { get; protected set; }

        /// <summary>
        /// Gets the friendly name of the error type.
        /// </summary>
        public string Error => this.Type.GetCustomName();
    }
}
