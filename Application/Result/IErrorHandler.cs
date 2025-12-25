// ===============================
// FILE: Application/Result/IErrorHandler.cs
// (Corrección: elimina dependencia de IErrorLogCreate para evitar acoplamiento Application↔Infrastructure)
// ===============================
namespace Application.Result
{
    public interface IErrorHandler
    {
        void LoadErrorMappings(string filePath);

        // Un solo punto de entrada para fallos: clasifica y construye Operation<T>
        Operation<T> Fail<T>(Exception? ex, string? errorMessage = null);

        Operation<T> Business<T>(string errorMessage);

        bool Any();
    }
}
