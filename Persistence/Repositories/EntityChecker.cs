using Domain.Interfaces.Entity;
using Persistence.Context.Interface;

namespace Persistence.Repositories
{
    /// <summary>
    /// Centraliza la validación de Id y la búsqueda por Id para evitar duplicación (DRY)
    /// y comportamientos divergentes entre capas/clases.
    /// </summary>
    public abstract class EntityChecker<T> : Read<T> where T : class, IEntity
    {
        private readonly Func<string, bool> _idValidator;

        protected EntityChecker(IUnitOfWork unitOfWork, Func<string, bool>? idValidator = null)
            : base(unitOfWork)
        {
            // Un solo punto de verdad para la validación de Id dentro de Persistence.
            // Si mañana el Id deja de ser GUID, reemplazas este validador en un solo lugar
            // (pasándolo por DI en los repos concretos o en el composition root).
            _idValidator = idValidator ?? (id => Guid.TryParse(id, out _));
        }

        protected async Task<T?> HasEntity(string id)
        {
            var results = await ReadFilter(e => e.Id == id);
            return results?.FirstOrDefault();
        }

        /// <summary>
        /// Valida el id de forma consistente y evita "silencios" con un motivo explícito.
        /// Mantiene compatibilidad retornando null, pero permite diagnosticar via error.
        /// </summary>
        protected async Task<T?> HasId(string id)
        {
            if (!TryValidateId(id, out _))
            {
                // Mantengo la semántica original (null) para no romper repos existentes.
                // Si quieres, puedes cambiar a lanzar excepción o retornar un Result/Operation.
                return null;
            }

            return await HasEntity(id);
        }

        /// <summary>
        /// Único método de validación en esta capa.
        /// </summary>
        protected bool TryValidateId(string? id, out string error)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                error = "Id cannot be null, empty, or whitespace.";
                return false;
            }

            if (!_idValidator(id))
            {
                error = "Id format is invalid.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
