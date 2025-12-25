using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Pagination;
using Application.Result;
using Domain.Interfaces.Entity;
using FluentAssertions;
using Infrastructure.Repositories.Abstract.CRUD.Query.Read;
using Marketing.Tests.Integration.Db;
using Marketing.Tests.Integration.TestEntities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence.Context.Interface;
using Persistence.CreateStructure.Constants.ColumnType;
using Xunit;

namespace Marketing.Tests.Integration;

public sealed class ReadRepositoryTests
{
    // -------------------------
    // Test repository + entity
    // -------------------------

    // TestEntity is in your test project (recommended) and mapped in TestDataContext.
    // It implements IEntity (Id + Active).
    // namespace Marketing.Tests.Integration.TestEntities

    private sealed class TestReadRepo : ReadRepository<TestEntity>
    {
        public int ApplyCursorFilterCallCount { get; private set; }

        public TestReadRepo(IUnitOfWork uow, Func<IQueryable<TestEntity>, IOrderedQueryable<TestEntity>> orderBy)
            : base(uow, orderBy)
        {
        }

        protected override IQueryable<TestEntity> ApplyCursorFilter(IQueryable<TestEntity> query, string cursor)
        {
            ApplyCursorFilterCallCount++;

            // Cursor logic: return items strictly after cursor based on ordinal comparison of string IDs.
            return query.Where(x => string.CompareOrdinal(x.Id, cursor) > 0);
        }

        protected override string? BuildNextCursor(List<TestEntity> items, int size)
        {
            // Defensive: production base doesn't validate pageSize.
            if (size <= 0) return null;

            // Next cursor exists if we fetched more than requested (pageSize+1).
            return items.Count > size ? items[size - 1].Id : null;
        }
    }

    // -------------------------
    // EF + UoW helpers
    // -------------------------

    private static async Task<(SqliteConnection conn, TestDataContext ctx)> CreateContextAsync()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        await conn.OpenAsync();

        var options = new DbContextOptionsBuilder()
            .UseSqlite(conn)
            .Options;

        IColumnTypes columnTypes = new TestColumnTypes();
        var ctx = new TestDataContext(options, columnTypes);

        await ctx.Database.EnsureCreatedAsync();
        return (conn, ctx);
    }

    private static IUnitOfWork CreateUnitOfWork(TestDataContext ctx)
    {
        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Context).Returns(ctx);
        return uow.Object;
    }

    private static async Task SeedAsync(TestDataContext ctx, params (string id, bool active)[] rows)
    {
        ctx.Set<TestEntity>().AddRange(rows.Select(r => new TestEntity { Id = r.id, Active = r.active }));
        await ctx.SaveChangesAsync();
    }

    private static Func<IQueryable<TestEntity>, IOrderedQueryable<TestEntity>> OrderByIdAsc()
        => q => q.OrderBy(x => x.Id);

    // -------------------------
    // GetAllMembers
    // -------------------------

    [Fact]
    public async Task GetAllMembers_When_empty_returns_empty_items_next_null_total_0()
    {
        var (conn, ctx) = await CreateContextAsync();
        await using var _ = ctx;
        await using var __ = conn;

        var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());

        var op = await repo.GetAllMembers();

        op.IsSuccessful.Should().BeTrue();
        op.Data.Should().NotBeNull();
        op.Data!.Items.Should().BeEmpty();
        op.Data.NextCursor.Should().BeNull();
        op.Data.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAllMembers_When_has_rows_returns_all_items_and_totalcount()
    {
        var (conn, ctx) = await CreateContextAsync();
        await using var _ = ctx;
        await using var __ = conn;

        await SeedAsync(ctx,
            ("003", true),
            ("001", true),
            ("002", false));

        var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());

        var op = await repo.GetAllMembers();

        op.IsSuccessful.Should().BeTrue();
        op.Data!.TotalCount.Should().Be(3);
        op.Data.NextCursor.Should().BeNull();

        // No explicit ordering in GetAllMembers (it uses Set<T>().AsNoTracking()) :contentReference[oaicite:1]{index=1}
        op.Data.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllMembers_Should_use_AsNoTracking_no_tracked_entities_created_by_query()
    {
        // Important: use a fresh context for query so no existing tracked entries from seeding.
        var (conn, ctx1) = await CreateContextAsync();
        await using var __ = conn;

        await SeedAsync(ctx1, ("001", true), ("002", true));
        await ctx1.DisposeAsync();

        var options = new DbContextOptionsBuilder()
            .UseSqlite(conn)
            .Options;

        IColumnTypes columnTypes = new TestColumnTypes();
        await using var ctx2 = new TestDataContext(options, columnTypes);

        var repo = new TestReadRepo(CreateUnitOfWork(ctx2), OrderByIdAsc());

        ctx2.ChangeTracker.Entries<TestEntity>().Should().BeEmpty();

        _ = await repo.GetAllMembers();

        // AsNoTracking() should not create tracked entries
        ctx2.ChangeTracker.Entries<TestEntity>().Should().BeEmpty();
    }

    // -------------------------
    // GetPageAsync - paging + next cursor
    // -------------------------

    [Fact]
    public async Task GetPageAsync_When_pageSize_less_than_total_returns_pageSize_items_and_next_cursor()
    {
        var (conn, ctx) = await CreateContextAsync();
        await using var _ = ctx;
        await using var __ = conn;

        await SeedAsync(ctx, ("001", true), ("002", true), ("003", true), ("004", true), ("005", true));

        var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());

        var op = await repo.GetPageAsync(filter: null, cursor: null, pageSize: 2);

        op.IsSuccessful.Should().BeTrue();
        op.Data!.TotalCount.Should().Be(5); // count computed before cursor, after filter :contentReference[oaicite:2]{index=2}
        op.Data.Items.Select(x => x.Id).Should().Equal("001", "002");
        op.Data.NextCursor.Should().Be("002"); // next cursor = items[pageSize-1] when more than pageSize
    }

    [Fact]
    public async Task GetPageAsync_When_pageSize_equals_total_returns_all_items_and_next_null()
    {
        var (conn, ctx) = await CreateContextAsync();
        await using var _ = ctx;
        await using var __ = conn;

        await SeedAsync(ctx, ("001", true), ("002", true), ("003", true));

        var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());

        var op = await repo.GetPageAsync(filter: null, cursor: null, pageSize: 3);

        op.IsSuccessful.Should().BeTrue();
        op.Data!.TotalCount.Should().Be(3);
        op.Data.Items.Select(x => x.Id).Should().Equal("001", "002", "003");
        op.Data.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task GetPageAsync_When_pageSize_greater_than_total_returns_all_items_and_next_null()
    {
        var (conn, ctx) = await CreateContextAsync();
        await using var _ = ctx;
        await using var __ = conn;

        await SeedAsync(ctx, ("001", true), ("002", true));

        var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());

        var op = await repo.GetPageAsync(filter: null, cursor: null, pageSize: 10);

        op.IsSuccessful.Should().BeTrue();
        op.Data!.TotalCount.Should().Be(2);
        op.Data.Items.Select(x => x.Id).Should().Equal("001", "002");
        op.Data.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task GetPageAsync_When_empty_dataset_returns_empty_and_next_null_total_0()
    {
        var (conn, ctx) = await CreateContextAsync();
        await using var _ = ctx;
        await using var __ = conn;

        var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());

        var op = await repo.GetPageAsync(filter: null, cursor: null, pageSize: 2);

        op.IsSuccessful.Should().BeTrue();
        op.Data!.TotalCount.Should().Be(0);
        op.Data.Items.Should().BeEmpty();
        op.Data.NextCursor.Should().BeNull();
    }

    // -------------------------
    // Cursor behavior
    // -------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetPageAsync_When_cursor_is_null_or_empty_should_not_apply_cursor_filter(string? cursor)
    {
        var (conn, ctx) = await CreateContextAsync();
        await using var _ = ctx;
        await using var __ = conn;

        await SeedAsync(ctx, ("001", true), ("002", true), ("003", true));

        var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());

        var op = await repo.GetPageAsync(filter: null, cursor: cursor, pageSize: 2);

        repo.ApplyCursorFilterCallCount.Should().Be(0);
        op.IsSuccessful.Should().BeTrue();
        op.Data!.Items.Select(x => x.Id).Should().Equal("001", "002");
    }

    // -------------------------
    // Filter behavior
    // -------------------------

    [Fact]
    public async Task GetPageAsync_When_filter_is_provided_should_filter_items_and_TotalCount_reflects_filtered_count()
    {
        var (conn, ctx) = await CreateContextAsync();
        await using var _ = ctx;
        await using var __ = conn;

        await SeedAsync(ctx,
            ("001", true),
            ("002", false),
            ("003", true),
            ("004", false),
            ("005", true));

        var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());

        Expression<Func<TestEntity, bool>> filter = x => x.Active;

        var op = await repo.GetPageAsync(filter, cursor: null, pageSize: 10);

        op.IsSuccessful.Should().BeTrue();
        op.Data!.TotalCount.Should().Be(3); // count after filter, before cursor :contentReference[oaicite:4]{index=4}
        op.Data.Items.Select(x => x.Id).Should().Equal("001", "003", "005");
        op.Data.NextCursor.Should().BeNull();
    }


    // -------------------------
    // Cancellation (GetAllMembers only)
    // -------------------------

    [Fact]
    public async Task GetAllMembers_When_cancelled_should_throw_TaskCanceledException_or_OperationCanceledException()
    {
        var (conn, ctx) = await CreateContextAsync();
        await using var _ = ctx;
        await using var __ = conn;

        await SeedAsync(ctx, ("001", true), ("002", true));

        var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Func<Task> act = async () => await repo.GetAllMembers(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
