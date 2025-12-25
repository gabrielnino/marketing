namespace Persistence.CreateStructure.Constants.ColumnType.Database
{
    /// <summary>
    /// SQLite-specific column type mappings and value-generation settings.
    /// </summary>
    public class SQLite : IColumnTypes
    {
        /// <inheritdoc/>
        public string Integer => "INTEGER";

        /// <inheritdoc/>
        public string Long => "INTEGER";

        /// <inheritdoc/>
        public string TypeBool => "INTEGER";  // SQLite stores booleans as integers (0 or 1)

        /// <inheritdoc/>
        public string TypeTime => "TEXT";     // ISO-8601 strings for times only

        /// <inheritdoc/>
        public string TypeDateTime => "TEXT"; // ISO-8601 strings for DateTime values

        /// <inheritdoc/>
        public string TypeDateTimeOffset => "TEXT"; // ISO-8601 strings with timezone offset

        /// <inheritdoc/>
        public string TypeVar50 => "TEXT";    // SQLite ignores length constraints on TEXT

        /// <inheritdoc/>
        public string TypeVar => "TEXT";

        /// <inheritdoc/>
        public string TypeVar150 => "TEXT";

        /// <inheritdoc/>
        public string TypeVar64 => "TEXT";

        /// <inheritdoc/>
        public string TypeBlob => "BLOB";

        /// <inheritdoc/>
        public string Strategy => "Sqlite:Autoincrement";

        /// <inheritdoc/>
        public object? SqlStrategy => true;   // Enable AUTOINCREMENT on INTEGER PRIMARY KEY

        /// <inheritdoc/>
        public string Name => string.Empty;   // No charset concept in SQLite

        /// <inheritdoc/>
        public object? Value => null;
    }
}
