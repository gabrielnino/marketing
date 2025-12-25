using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Marketing.Tests.Integration.Db;
using Persistence.CreateStructure.Constants.ColumnType;
using Xunit;
using Marketing.Tests.Integration.TestEntities;

namespace Marketing.Tests
{
    public class PagingTests
    {
        [Fact]
        public async Task Smoke_Can_create_db_and_insert()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();

            var options = new DbContextOptionsBuilder()
                .UseSqlite(conn)
                .Options;

            IColumnTypes columnTypes = new TestColumnTypes();
            await using var ctx = new TestDataContext(options, columnTypes);

            // Important: don't call ctx.Initialize() in tests
            await ctx.Database.EnsureCreatedAsync();

            ctx.Set<TestEntity>()
               .Add(new() { Id = "001", Active = true });

            await ctx.SaveChangesAsync();

            var count = await ctx.Set<TestEntity>().CountAsync();
            count.Should().Be(1);
        }
    }
}