namespace Application.Result.EnumType.Extensions
{
    using Application.Constants;
    using System;

    /// <summary>
    /// Marks an enum field with a custom name and description.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnumMetadata : Attribute
    {
        /// <summary>
        /// The display name for the enum value.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A short description explaining the enum value.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Initializes the attribute with the given name and description.
        /// </summary>
        /// <param name="name">The custom name (required).</param>
        /// <param name="description">The custom description (required).</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if name or description is null, empty, or whitespace.
        /// </exception>
        public EnumMetadata(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(Messages.EnumMetadata.ForNameOrDescription);
            }

            Name = name;
            Description = description;
        }
    }
}
