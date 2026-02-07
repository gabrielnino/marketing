using Application.Result;

namespace Application.PixVerse.Http
{
    public interface IPixVerseRequestHandler
    {
        Task<Operation<T>> GetAsync<T>(string path, CancellationToken ct = default);
        Task<Operation<T>> PostAsync<T>(string path, HttpContent? content, CancellationToken ct = default);
        Task<Operation<T>> SendAsync<T>(HttpRequestMessage request, CancellationToken ct = default);
        
        // Helper to download files (returns success with FileInfo on valid download)
        Task<Operation<FileInfo>> DownloadFileAsync(string url, string destinationPath, CancellationToken ct = default);
    }
}
