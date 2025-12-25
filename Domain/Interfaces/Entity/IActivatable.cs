namespace Domain.Interfaces.Entity
{
    /// <summary>
    /// Defines a contract for entities that can be set the state.
    /// </summary>
    public interface IActivatable
    {
        /// <summary>
        ///  Gets or sets a value indicating whether this entity is active.
        /// </summary>
        bool Active { get; set; }
    }
}
