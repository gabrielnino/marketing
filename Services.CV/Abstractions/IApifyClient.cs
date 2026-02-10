using Application.Apify.Models;
using Application.Result;

namespace Services.CV.Abstractions
{
    public interface IApifyClient
    {
        // Builds
        Task<Operation<PaginatedList<ActorBuild>>> GetBuildsAsync(int limit = 20, int offset = 0, bool desc = false, CancellationToken ct = default);
        Task<Operation<ActorBuild>> GetBuildAsync(string buildId, bool waitForFinish = false, CancellationToken ct = default);
        Task<Operation<bool>> DeleteBuildAsync(string buildId, CancellationToken ct = default);
        Task<Operation<ActorBuild>> AbortBuildAsync(string buildId, CancellationToken ct = default);
        Task<Operation<string>> GetBuildLogAsync(string buildId, CancellationToken ct = default);
        Task<Operation<string>> GetBuildOpenApiDefAsync(string buildId, CancellationToken ct = default); // Returns JSON string

        // Runs
        Task<Operation<PaginatedList<ActorRun>>> GetRunsAsync(int limit = 20, int offset = 0, bool desc = false, CancellationToken ct = default);
        Task<Operation<ActorRun>> GetRunAsync(string runId, bool waitForFinish = false, CancellationToken ct = default);
        Task<Operation<ActorRun>> UpdateRunAsync(string runId, ActorRunUpdate update, CancellationToken ct = default);
        Task<Operation<bool>> DeleteRunAsync(string runId, CancellationToken ct = default);
        Task<Operation<ActorRun>> AbortRunAsync(string runId, CancellationToken ct = default);
        Task<Operation<ActorRun>> MetamorphRunAsync(string runId, string targetActorId, object? input = null, string? build = null, CancellationToken ct = default);
        Task<Operation<ActorRun>> RebootRunAsync(string runId, CancellationToken ct = default);
        Task<Operation<ActorRun>> ResurrectRunAsync(string runId, CancellationToken ct = default);
        Task<Operation<bool>> ChargeRunAsync(string runId, string eventName, int count = 1, CancellationToken ct = default);
        Task<Operation<List<Dictionary<string, object>>>> GetRunDatasetItemsAsync(string runId, CancellationToken ct = default);
        Task<Operation<PaginatedList<Dataset>>> GetDatasetsAsync(int limit = 20, int offset = 0, bool desc = false, bool unnamed = false, CancellationToken ct = default);
        Task<Operation<List<DatasetItem>>> GetDatasetItemsAsync(string datasetId, int limit = 1000, int offset = 0, CancellationToken ct = default);
    }
}
