using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface ISubmitTransition
    {
        Task<Operation<JobSubmitted>> SubmitTransitionAsync(
        Transition request,
        CancellationToken ct = default);
    }
}
