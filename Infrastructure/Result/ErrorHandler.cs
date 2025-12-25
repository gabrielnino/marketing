// ===============================
// FILE: Infrastructure/Result/ErrorHandler.cs
// (Corrección: SRP + DIP: clasifica + crea Operation<T>; el persist/log se delega a IErrorLogger)
// ===============================
using Application.Result;
using Application.Result.Error;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Infrastructure.Result
{
    public sealed class ErrorHandler : IErrorHandler
    {
        private readonly IErrorLogger _errorLogger;

        // Nota: esto sigue siendo estático; si quieres, puedes volverlo instancia para testear más fácil.
        private static readonly Lazy<ConcurrentDictionary<string, string>> ErrorMappings
            = new(() => new ConcurrentDictionary<string, string>());

        private static readonly IDictionary<string, string> DefaultMappings = new Dictionary<string, string>
        {
            { "SqliteException",      "DatabaseStrategy" },
            { "HttpRequestException", "NetworkErrorStrategy" },
            { "JsonException",        "InvalidDataStrategy" },
            { "Exception",            "UnexpectedErrorStrategy" }
        };

        public ErrorHandler(IErrorLogger errorLogger)
        {
            _errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
        }

        public Operation<T> Fail<T>(Exception? ex, string? errorMessage = null)
        {
            if (ex is null)
                return new NullExceptionStrategy<T>().CreateFailure("Exception is null.");

            if (ErrorMappings.Value.IsEmpty)
                return new NullExceptionStrategy<T>().CreateFailure("ErrorMappings is not loaded or empty.");

            if (!ErrorMappings.Value.TryGetValue(ex.GetType().Name, out var strategyName))
                return new NullExceptionStrategy<T>().CreateFailure($"No strategy matches exception type: {ex.GetType().Name}.");

            // 1) Crear el resultado (responsabilidad: clasificación + construcción)
            var strategy = CreateStrategyInstance<T>(strategyName);
            var op = string.IsNullOrWhiteSpace(errorMessage)
                ? strategy.CreateFailure()
                : strategy.CreateFailure(errorMessage);

            // 2) Registrar el error (responsabilidad separada: logging/persistencia)
            //    No bloquea (sin .Wait / .Result). Si falla el log, NO debe tumbar el flujo principal.
            _ = SafeLogAsync(ex);

            return op;
        }

        public Operation<T> Business<T>(string errorMessage)
            => new BusinessStrategy<T>().CreateFailure(errorMessage);

        public void LoadErrorMappings(string filePath)
        {
            foreach (var kv in DefaultMappings)
                ErrorMappings.Value[kv.Key] = kv.Value;

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Error mappings file not found: {filePath}");

            try
            {
                var jsonContent = File.ReadAllText(filePath);
                var mappings = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

                if (mappings is null || mappings.Count == 0)
                    throw new InvalidOperationException("ErrorMappings.json is empty or invalid.");

                foreach (var kvp in mappings)
                    ErrorMappings.Value[kvp.Key] = kvp.Value;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("ErrorMappings.json contains invalid JSON format.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while loading error mappings.", ex);
            }
        }

        public bool Any() => ErrorMappings.IsValueCreated && !ErrorMappings.Value.IsEmpty;

        private static IErrorCreationStrategy<T> CreateStrategyInstance<T>(string strategyName) =>
            strategyName switch
            {
                "NetworkErrorStrategy" => new NetworkErrorStrategy<T>(),
                "ConfigMissingStrategy" => new ConfigMissingStrategy<T>(),
                "InvalidDataStrategy" => new InvalidDataStrategy<T>(),
                "DatabaseStrategy" => new DatabaseStrategy<T>(),
                "UnexpectedErrorStrategy" => new UnexpectedErrorStrategy<T>(),
                _ => new UnexpectedErrorStrategy<T>()
            };

        private async Task SafeLogAsync(Exception ex)
        {
            try
            {
                await _errorLogger.LogAsync(ex).ConfigureAwait(false);
            }
            catch
            {
                // Intencional: no propagamos errores del logger.
                // Si quieres visibilidad, aquí puedes usar ILogger<ErrorHandler> (sin persistir).
            }
        }
    }
}
            