namespace Application.Result.EnumType.Extensions
{
    using Application.Constants;
    using System;
    using System.Reflection;

    /// <summary>
    /// Provides extension methods to read enum metadata.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Returns the custom name from the EnumMetadata attribute, or a default if not set.
        /// </summary>
        public static string GetCustomName<TEnum>(this TEnum enumValue)
            where TEnum : struct, Enum
        {
            return GetEnumMetadata(enumValue)?.Name ?? Messages.EnumExtensions.Unknown;
        }

        /// <summary>
        /// Returns the description from the EnumMetadata attribute, or a default if not set.
        /// </summary>
        public static string GetDescription<TEnum>(this TEnum enumValue)
            where TEnum : struct, Enum
        {
            return GetEnumMetadata(enumValue)?.Description ?? Messages.EnumExtensions.DescriptionNotAvailable;
        }

        /// <summary>
        /// Reads EnumMetadata from the given enum value using reflection.
        /// </summary>
        private static EnumMetadata? GetEnumMetadata<TEnum>(TEnum enumValue)
            where TEnum : Enum
        {
            var type = enumValue.GetType();
            var name = Enum.GetName(type, enumValue);
            if (name != null)
            {
                var field = type.GetField(name);
                if (field?.GetCustomAttribute<EnumMetadata>(false) is EnumMetadata attribute)
                {
                    return attribute;
                }
            }

            return null;
        }
    }
}
