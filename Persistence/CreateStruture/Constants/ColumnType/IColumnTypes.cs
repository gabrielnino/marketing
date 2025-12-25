namespace Persistence.CreateStructure.Constants.ColumnType
{
    /// <summary>
    /// Defines a contract for column type mappings.
    /// </summary>
    public interface IColumnTypes
    {
        /// <summary>
        /// Gets the SQL column type used to store boolean values.
        /// </summary>
        string TypeBool { get; }

        /// <summary>
        /// Gets the SQL column type used to store time values (without date).
        /// </summary>
        string TypeTime { get; }

        /// <summary>
        /// Gets the SQL column type used to store datetime values.
        /// </summary>
        string TypeDateTime { get; }

        /// <summary>
        /// Gets the SQL column type used to store datetime values with timezone offset.
        /// </summary>
        string TypeDateTimeOffset { get; }

        /// <summary>
        /// Gets the SQL column type used to store variable text (default length).
        /// </summary>
        string TypeVar { get; }

        /// <summary>
        /// Gets the SQL column type used to store variable text up to 50 characters.
        /// </summary>
        string TypeVar50 { get; }

        /// <summary>
        /// Gets the SQL column type used to store variable text up to 150 characters.
        /// </summary>
        string TypeVar150 { get; }

        /// <summary>
        /// Gets the SQL column type used to store fixed text up to 64 characters.
        /// </summary>
        string TypeVar64 { get; }

        /// <summary>
        /// Gets the SQL column type used to store binary large object (BLOB) data.
        /// </summary>
        string TypeBlob { get; }

        /// <summary>
        /// Gets the SQL column type used to store 32-bit integer values.
        /// </summary>
        string Integer { get; }

        /// <summary>
        /// Gets the SQL column type used to store 64-bit integer values.
        /// </summary>
        string Long { get; }

        /// <summary>
        /// Gets the metadata key name for configuring value strategy.
        /// </summary>
        string Strategy { get; }

        /// <summary>
        /// Gets the value associated with the <see cref="Strategy"/> metadata key.
        /// </summary>
        object? SqlStrategy { get; }

        /// <summary>
        /// Gets the metadata key name for database-specific settings (e.g., charset).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the value associated with the <see cref="Name"/> metadata key.
        /// </summary>
        object? Value { get; }
    }
}
