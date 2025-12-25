using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marketing.Tests.Integration
{
    using System.Threading.Tasks;
    using global::Marketing.Tests.Integration.Db;
    //using Marketing.Tests.Integration.Db;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Persistence.CreateStructure.Constants.ColumnType;

    //namespace Marketing.Tests.Integration;

    /// <summary>
    /// Creates a fully-initialized SQLite in-memory DbContext
    /// with all required database functions registered.
    /// </summary>
    internal static class TestDbContextFactory
    {
        public static async Task<(SqliteConnection Connection, TestDataContext Context)> CreateContextAsync()
        {
            // 1️⃣ Create in-memory SQLite connection
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            // 2️⃣ Register DB function used by DataContext
            // Must be registered BEFORE DbContext is created
            connection.CreateFunction<string, string, int>(
                "StringCompareOrdinal",
                (a, b) => string.CompareOrdinal(a, b)
            );

            // 3️⃣ Configure EF Core
            var options = new DbContextOptionsBuilder()
                .UseSqlite(connection)
                .Options;

            // 4️⃣ Provide test column types
            IColumnTypes columnTypes = new TestColumnTypes();

            // 5️⃣ Create context
            var context = new TestDataContext(options, columnTypes);

            // 6️⃣ Ensure schema is created (DO NOT call Migrate in tests)
            await context.Database.EnsureCreatedAsync();

            return (connection, context);
        }
    }

}
