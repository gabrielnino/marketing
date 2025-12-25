using Persistence.CreateStructure.Constants.ColumnType;

namespace Marketing.Tests.Integration.Db;

internal sealed class TestColumnTypes : IColumnTypes
{
    // --- Generic aliases (if used anywhere) ---
    public string Id => TypeVar64;
    public string Guid => TypeVar64;
    public string String => TypeVar;
    public string ShortString => TypeVar50;
    public string LongString => TypeVar;

    public string Bool => TypeBool;
    public string Int => Integer;
    public string Long => Integer;
    public string Decimal => "NUMERIC";
    public string DateTime => TypeDateTime;

    // --- Interface members you showed ---
    public string TypeBool => "INTEGER";           // SQLite stores booleans as integer 0/1
    public string TypeTime => "TEXT";              // or "NUMERIC" depending on your conventions
    public string TypeDateTime => "TEXT";          // ISO-8601
    public string TypeDateTimeOffset => "TEXT";    // ISO-8601 with offset
    public string TypeVar => "TEXT";
    public string TypeVar50 => "TEXT";
    public string TypeVar150 => "TEXT";
    public string TypeVar64 => "TEXT";
    public string TypeBlob => "BLOB";
    public string Integer => "INTEGER";

    // These look like they model a "strategy" object in your design.
    // For tests, return harmless placeholders.
    public string Strategy => "SQLiteTest";
    public object? SqlStrategy => null;

    public string Name => "SQLiteTestColumnTypes";
    public object? Value => null;
}
