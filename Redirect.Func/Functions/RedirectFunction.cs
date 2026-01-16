using System.Net;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Redirect.Func.Functions;

public sealed class RedirectFunction
{
    private readonly TableClient _tableClient;
    private readonly QueueClient _queueClient;
    private readonly ILogger<RedirectFunction> _logger;

    public RedirectFunction(
        TableServiceClient tableServiceClient,
        QueueServiceClient queueServiceClient,
        ILogger<RedirectFunction> logger)
    {
        _tableClient = tableServiceClient.GetTableClient("TrackedLinks");
        _queueClient = queueServiceClient.GetQueueClient("trackedlink-visits");
        _logger = logger;
    }

    [Function("Redirect")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{code}")]
        HttpRequestData req,
        string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            // PartitionKey fijo = "LINK"
            var entity = await _tableClient.GetEntityAsync<TableEntity>(
                partitionKey: "LINK",
                rowKey: code);

            var targetUrl = entity.Value.GetString("TargetUrl");

            if (string.IsNullOrWhiteSpace(targetUrl))
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            // Encolar visita (NO bloqueante)
            await EnqueueVisitAsync(code, req);

            var response = req.CreateResponse(HttpStatusCode.Found);
            response.Headers.Add("Location", targetUrl);
            return response;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redirect error for code {Code}", code);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    private async Task EnqueueVisitAsync(string code, HttpRequestData req)
    {
        try
        {
            var ip =
                req.Headers.TryGetValues("X-Forwarded-For", out var values)
                    ? values.FirstOrDefault()
                    : "unknown";

            var payload = new
            {
                Code = code,
                TimestampUtc = DateTime.UtcNow,
                Ip = ip
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            await _queueClient.SendMessageAsync(Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(json)));
        }
        catch (Exception ex)
        {
            // No rompemos el redirect por métricas
            _logger.LogWarning(ex, "Failed to enqueue visit for {Code}", code);
        }
    }
}
