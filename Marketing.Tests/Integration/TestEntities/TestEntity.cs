using Domain.Interfaces.Entity;

namespace Marketing.Tests.Integration.TestEntities;

internal sealed class TestEntity : IEntity
{
    public string Id { get; set; } = default!;
    public bool Active { get; set; }
}
