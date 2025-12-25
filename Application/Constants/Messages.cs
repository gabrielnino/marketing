namespace Application.Constants
{
    public static class Messages
    {
        public static class InvalidOperation
        {
            public const string NullMessage = "The 'message' parameter cannot be null, empty, or whitespace.";
        }

        public static class Operation
        {
            public const string InvalidOperation = "This method can only be used if the value of IsSuccessful is false.";
        }


        public static class EnumExtensions
        {
            public const string Unknown = "UNKNOWN";
            public const string DescriptionNotAvailable = "Description not available.";
            public const string NoEnumValueFound = "No enum value found for {0} in {1}";
        }

        public static class EnumMetadata
        {
            public const string ForNameOrDescription = "For name or description, null, empty, and whitespace are not allowed.";
        }
    }
}
