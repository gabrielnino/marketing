using Domain;

namespace Services.Abstractions.AutoIt
{
    public interface IAutoItRunner
    {
        public Task<AutoItRunnerResult> RunAsync(
            TimeSpan timeout,
            string imagePath,
            bool useAutoItInterpreter = false,
            CancellationToken cancellationToken = default);
    }
}
