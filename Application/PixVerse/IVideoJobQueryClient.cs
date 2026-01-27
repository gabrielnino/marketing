using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface IVideoJobQueryClient
    {
        Task<Operation<JobStatus>> GetStatusAsync(
            long jobId,
            CancellationToken ct = default);

        Task<Operation<JobResult>> GetResultAsync(
            long jobId,
            CancellationToken ct = default);
    }
}
