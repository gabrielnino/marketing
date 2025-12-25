namespace Domain.Interfaces.Entity
{
    /// <summary>
    /// Defines a contract for entities that can be unique.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        ///  Gets or sets the entity’s ID.
        /// </summary>
        string Id { get; }
    }
}
