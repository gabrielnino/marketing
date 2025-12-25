using Domain.Interfaces.Entity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Domain
{
    /// <summary>
    /// Represents a Entity.
    /// </summary>
    public class Entity : IEntity
    {

        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// Required by the <see cref="IIdentifiable"/> interface.
        /// </summary>
        [Key]
        [Required(ErrorMessage = "Id is required.")]
        public required string Id { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier for the entity.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is empty or consists only of whitespace.</exception>
        [SetsRequiredMembers]
        public Entity(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Id cannot be empty or whitespace.", nameof(id));
            }
            Id = id;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this entity is active.
        /// </summary>
        /// <value>
        /// <c>true</c> if the entity is active; otherwise, <c>false</c>.
        /// </value>
        public bool Active { get; set; }
    }
}
