using Microsoft.EntityFrameworkCore;
using Persistence.Context.Implementation;
using Persistence.CreateStructure.Constants.ColumnType;
using Marketing.Tests.Integration.TestEntities;

namespace Marketing.Tests.Integration.Db;

internal sealed class TestDataContext : DataContext
{
    public TestDataContext(DbContextOptions options, IColumnTypes columnTypes)
        : base(options, columnTypes)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Test-only mapping
        modelBuilder.Entity<TestEntity>(b =>
        {
            b.ToTable("TestEntities");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasMaxLength(64);
            b.Property(x => x.Active);
        });
    }
}
