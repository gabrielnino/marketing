namespace Application.Result.Exceptions
{
    using Application.Constants;

    public class InvalidOperation : Exception
    {
        public InvalidOperation(string message) : base(message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException(nameof(message), Messages.InvalidOperation.NullMessage);
            }
        }
    }
}
