using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Persistence.Context.Interceptors
{
    /// <summary>
    /// Adds a custom SQL function when a SQLite connection opens.
    /// </summary>
    public class SqliteFunctionInterceptor : DbConnectionInterceptor
    {
        /// <summary>
        /// Called after a connection opens synchronously.
        /// Registers the custom function if using SQLite.
        /// </summary>
        public override void ConnectionOpened(
            DbConnection connection,
            ConnectionEndEventData eventData)
        {
            if (connection is SqliteConnection sqlite)
                RegisterFunction(sqlite);

            base.ConnectionOpened(connection, eventData);
        }

        /// <summary>
        /// Called after a connection opens asynchronously.
        /// Registers the custom function if using SQLite.
        /// </summary>
        public override async Task ConnectionOpenedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            if (connection is SqliteConnection sqlite)
                RegisterFunction(sqlite);

            await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
        }

        /// <summary>
        /// Defines a SQLite function "StringCompareOrdinal" 
        /// that compares two strings using ordinal rules.
        /// </summary>
        private static void RegisterFunction(SqliteConnection sqlite)
        {
            sqlite.CreateFunction<string, string, int>(
                "StringCompareOrdinal",
                (a, b) => a == b ? 0
                            : string.Compare(a, b, StringComparison.Ordinal) > 0 ? 1
                            : -1
            );
        }
    }
}
