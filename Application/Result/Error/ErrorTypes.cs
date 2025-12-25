using Application.Result.EnumType.Extensions;

namespace Application.Result.Error
{
    public enum ErrorTypes
    {
        [EnumMetadata("NONE", "No error has occurred.")]
        None,

        [EnumMetadata("BUSINESS_VALIDATION_ERROR", "Occurs when business logic validation fails.")]
        BusinessValidation,

        [EnumMetadata("DATABASE_ERROR", "Occurs when an error happens during database interaction.")]
        Database,

        [EnumMetadata("UNEXPECTED_ERROR", "Occurs for any unexpected or unclassified error.")]
        Unexpected,

        [EnumMetadata("DATA_SUBMITTED_INVALID", "Occurs when the submitted data is invalid.")]
        InvalidData,

        [EnumMetadata("CONFIGURATION_MISSING_ERROR", "Occurs when a required configuration is missing.")]
        ConfigMissing,

        [EnumMetadata("NETWORK_ERROR", "Occurs due to a network connectivity issue.")]
        Network,

        [EnumMetadata("USER_INPUT_ERROR", "Occurs when user input is invalid.")]
        UserInput,

        [EnumMetadata("NONE_FOUND_ERROR", "Occurs when a requested resource is not found.")]
        NotFound,

        [EnumMetadata("AUTHENTICATION_ERROR", "Occurs when user authentication fails.")]
        Authentication,

        [EnumMetadata("AUTHORIZATION_ERROR", "Occurs when the user is not authorized to perform the action.")]
        Authorization,

        [EnumMetadata("RESOURCE_ERROR", "Occurs when allocating or accessing a resource fails.")]
        Resource,

        [EnumMetadata("TIMEOUT_ERROR", "Occurs when an operation times out.")]
        Timeout,

        [EnumMetadata("NULL_EXCEPTION_STRATEGY", "Occurs when the error-mappings dictionary is uninitialized.")]
        NullExceptionStrategy
    }
}

