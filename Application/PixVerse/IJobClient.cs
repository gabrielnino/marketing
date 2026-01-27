using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface IJobClient
    {
        Task<Operation<JobResult>> WaitForCompletionAsync(
            long jobId,
            CancellationToken ct = default);
    }
}
